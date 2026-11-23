using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

namespace HealthServer;

internal enum OtpVerifyStatus
{
    Success,
    InvalidOrExpired,
    TooManyAttempts
}

internal sealed record OtpVerifyResult(
    OtpVerifyStatus Status,
    int RemainingAttempts);

internal sealed class PasswordResetRepository
{
    private readonly DatabaseHelper _database;

    public PasswordResetRepository(DatabaseHelper database)
    {
        _database = database;
    }

    /// <summary>
    /// Đóng mọi yêu cầu đang mở của user (nếu có) và tạo yêu cầu OTP mới (hạn 15 phút).
    /// </summary>
    public async Task CreateOrReplaceRequestAsync(
        int userId,
        byte[] otpHash,
        DateTime otpExpiresAtUtc)
    {
        if (otpHash.Length != 32)
            throw new ArgumentException("OtpHash phải dài đúng 32 bytes (SHA-256).", nameof(otpHash));

        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            using (var revokeCmd = connection.CreateCommand())
            {
                revokeCmd.Transaction = transaction;
                revokeCmd.CommandText = """
                    UPDATE dbo.PasswordResetRequests
                    SET RevokedAt = SYSUTCDATETIME()
                    WHERE UserID = @UserID
                      AND UsedAt IS NULL
                      AND RevokedAt IS NULL;
                    """;
                revokeCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                await revokeCmd.ExecuteNonQueryAsync();
            }

            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = """
                    INSERT INTO dbo.PasswordResetRequests (UserID, OtpHash, OtpExpiresAt)
                    VALUES (@UserID, @OtpHash, @OtpExpiresAt);
                    """;
                insertCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                insertCmd.Parameters.Add("@OtpHash", SqlDbType.VarBinary, 32).Value = otpHash;
                insertCmd.Parameters.Add("@OtpExpiresAt", SqlDbType.DateTime2, 3).Value = otpExpiresAtUtc;
                await insertCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra OTP 6 số (tối đa 5 lần sai). Nếu đúng thì cấp resetTokenHash (hạn 10 phút).
    /// Không khóa Users.IsActive khi sai quá 5 lần — chỉ đóng yêu cầu OTP hiện tại.
    /// </summary>
    public async Task<OtpVerifyResult> VerifyOtpAndIssueResetTokenAsync(
        int userId,
        byte[] otpHash,
        byte[] resetTokenHash,
        DateTime resetTokenExpiresAtUtc)
    {
        if (otpHash.Length != 32)
            throw new ArgumentException("OtpHash phải dài đúng 32 bytes.", nameof(otpHash));
        if (resetTokenHash.Length != 32)
            throw new ArgumentException("ResetTokenHash phải dài đúng 32 bytes.", nameof(resetTokenHash));

        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            long requestId;
            byte[] storedOtpHash;
            DateTime otpExpiresAt;
            int failedAttempts;
            bool alreadyHasResetToken;

            using (var selectCmd = connection.CreateCommand())
            {
                selectCmd.Transaction = transaction;
                selectCmd.CommandText = """
                    SELECT RequestID, OtpHash, OtpExpiresAt, FailedAttempts, ResetTokenHash
                    FROM dbo.PasswordResetRequests WITH (UPDLOCK, HOLDLOCK)
                    WHERE UserID = @UserID
                      AND UsedAt IS NULL
                      AND RevokedAt IS NULL;
                    """;
                selectCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                await using var reader = await selectCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    await transaction.RollbackAsync();
                    return new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0);
                }

                requestId = reader.GetInt64(0);
                storedOtpHash = (byte[])reader[1];
                otpExpiresAt = reader.GetDateTime(2);
                failedAttempts = reader.GetInt32(3);
                alreadyHasResetToken = !reader.IsDBNull(4);
            }

            DateTime nowUtc = DateTime.UtcNow;

            if (alreadyHasResetToken || otpExpiresAt <= nowUtc)
            {
                await RevokeRequestAsync(connection, transaction, requestId);
                await transaction.CommitAsync();
                return new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0);
            }

            if (failedAttempts >= 5)
            {
                await RevokeRequestAsync(connection, transaction, requestId);
                await transaction.CommitAsync();
                return new OtpVerifyResult(OtpVerifyStatus.TooManyAttempts, 0);
            }

            bool isMatch = storedOtpHash.Length == otpHash.Length &&
                           CryptographicOperations.FixedTimeEquals(storedOtpHash, otpHash);

            if (!isMatch)
            {
                int nextFailed = failedAttempts + 1;
                bool lockRequest = nextFailed >= 5;

                using var failCmd = connection.CreateCommand();
                failCmd.Transaction = transaction;
                failCmd.CommandText = """
                    UPDATE dbo.PasswordResetRequests
                    SET FailedAttempts = @FailedAttempts,
                        RevokedAt = CASE WHEN @LockRequest = 1 THEN SYSUTCDATETIME() ELSE NULL END
                    WHERE RequestID = @RequestID;
                    """;
                failCmd.Parameters.Add("@FailedAttempts", SqlDbType.Int).Value = nextFailed;
                failCmd.Parameters.Add("@LockRequest", SqlDbType.Bit).Value = lockRequest;
                failCmd.Parameters.Add("@RequestID", SqlDbType.BigInt).Value = requestId;
                await failCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return lockRequest
                    ? new OtpVerifyResult(OtpVerifyStatus.TooManyAttempts, 0)
                    : new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 5 - nextFailed);
            }

            using (var issueCmd = connection.CreateCommand())
            {
                issueCmd.Transaction = transaction;
                issueCmd.CommandText = """
                    UPDATE dbo.PasswordResetRequests
                    SET ResetTokenHash = @ResetTokenHash,
                        ResetTokenExpiresAt = @ResetTokenExpiresAt
                    WHERE RequestID = @RequestID;
                    """;
                issueCmd.Parameters.Add("@ResetTokenHash", SqlDbType.VarBinary, 32).Value = resetTokenHash;
                issueCmd.Parameters.Add("@ResetTokenExpiresAt", SqlDbType.DateTime2, 3).Value = resetTokenExpiresAtUtc;
                issueCmd.Parameters.Add("@RequestID", SqlDbType.BigInt).Value = requestId;
                await issueCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return new OtpVerifyResult(OtpVerifyStatus.Success, 5 - failedAttempts);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Xác thực resetToken (dùng 1 lần, hạn 10 phút), cập nhật mật khẩu mới và trả về UserID để thu hồi phiên.
    /// </summary>
    public async Task<int?> ConsumeResetTokenAndUpdatePasswordAsync(
        byte[] resetTokenHash,
        string newPasswordHash,
        string newSalt,
        int iterations)
    {
        if (resetTokenHash.Length != 32)
            throw new ArgumentException("ResetTokenHash phải dài đúng 32 bytes.", nameof(resetTokenHash));

        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            int userId;

            using (var consumeCmd = connection.CreateCommand())
            {
                consumeCmd.Transaction = transaction;
                consumeCmd.CommandText = """
                    UPDATE dbo.PasswordResetRequests
                    SET UsedAt = SYSUTCDATETIME()
                    OUTPUT INSERTED.UserID
                    WHERE ResetTokenHash = @ResetTokenHash
                      AND UsedAt IS NULL
                      AND RevokedAt IS NULL
                      AND ResetTokenExpiresAt > SYSUTCDATETIME();
                    """;
                consumeCmd.Parameters.Add("@ResetTokenHash", SqlDbType.VarBinary, 32).Value = resetTokenHash;

                var result = await consumeCmd.ExecuteScalarAsync();
                if (result is not int foundUserId)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                userId = foundUserId;
            }

            using (var updatePwdCmd = connection.CreateCommand())
            {
                updatePwdCmd.Transaction = transaction;
                updatePwdCmd.CommandText = """
                    UPDATE dbo.Users
                    SET MatKhauBam = @PasswordHash,
                        Salt = @Salt,
                        SoVongLap = @Iterations,
                        SoLanSai = 0,
                        KhoaDenNgay = NULL
                    WHERE UserID = @UserID AND IsActive = 1;
                    """;
                updatePwdCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                updatePwdCmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 64).Value = newPasswordHash;
                updatePwdCmd.Parameters.Add("@Salt", SqlDbType.NVarChar, 32).Value = newSalt;
                updatePwdCmd.Parameters.Add("@Iterations", SqlDbType.Int).Value = iterations;

                if (await updatePwdCmd.ExecuteNonQueryAsync() != 1)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }

            await transaction.CommitAsync();
            return userId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task RevokeRequestAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long requestId)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            UPDATE dbo.PasswordResetRequests
            SET RevokedAt = SYSUTCDATETIME()
            WHERE RequestID = @RequestID;
            """;
        cmd.Parameters.Add("@RequestID", SqlDbType.BigInt).Value = requestId;
        await cmd.ExecuteNonQueryAsync();
    }
}