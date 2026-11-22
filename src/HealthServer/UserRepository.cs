using Microsoft.VisualBasic;
using System.Data;

namespace HealthServer;

internal sealed record LoginUser(
    int UserId,
    string PasswordHash,
    string Salt,
    int Iterations,
    bool IsActive,
    DateTime? LockedUntil);
internal sealed record AccountProfile(
    int UserId,
    string Username,
    string FullName,
    string? Email);
internal sealed class UserRepository
{
    private readonly DatabaseHelper _database;

    public UserRepository(DatabaseHelper database)
    {
        _database = database;
    }

    public async Task<LoginUser?> FindForLoginAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
            return null;

        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT UserID, MatKhauBam, Salt, SoVongLap, IsActive, KhoaDenNgay
            FROM dbo.Users
            WHERE TenDangNhap = @TenDangNhap;
            """;

        command.Parameters.Add("@TenDangNhap", SqlDbType.NVarChar, 50).Value = username;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new LoginUser(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetBoolean(4),
            reader.IsDBNull(5) ? null : reader.GetDateTime(5));
    }
    public async Task<AccountProfile?> GetProfileAsync(int userId)
{
    await using var connection = _database.CreateConnection();
    using var command = connection.CreateCommand();

    command.CommandText = """
        SELECT UserID, TenDangNhap, HoTen, Email
        FROM dbo.Users
        WHERE UserID = @UserID;
        """;

    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

    await connection.OpenAsync();
    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return null;

    return new AccountProfile(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.IsDBNull(3) ? null : reader.GetString(3));
}
public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? email)
{
    if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
    {
        throw new ArgumentException("Họ tên phải có từ 1 đến 100 ký tự.", nameof(fullName));
    }

    if (email is not null && email.Length > 254)
    {
        throw new ArgumentException("Email tối đa 254 ký tự.", nameof(email));
    }

    await using var connection = _database.CreateConnection();
    using var command = connection.CreateCommand();

    command.CommandText = """
        UPDATE dbo.Users
        SET HoTen = @FullName, Email = @Email
        WHERE UserID = @UserID;
        """;

    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
    command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
    command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value =
        (object?)email ?? DBNull.Value;

    await connection.OpenAsync();

    int affectedRows = await command.ExecuteNonQueryAsync();
    return affectedRows == 1;
}
    public async Task<int> CreateUserAsync(
        string username,
        string passwordHash,
        string salt,
        string fullName,
        string? email)
    {
        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO dbo.Users (TenDangNhap, MatKhauBam, Salt, HoTen, Email)
            OUTPUT INSERTED.UserID
            VALUES (@Username, @PasswordHash, @Salt, @FullName, @Email);
            """;

        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 64).Value = passwordHash;
        command.Parameters.Add("@Salt", SqlDbType.NVarChar, 32).Value = salt;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value =
            (object?)email ?? DBNull.Value;

        await connection.OpenAsync();

        return (int)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("Không nhận được UserID sau khi đăng ký."));
    }
    public async Task<bool> RecordLoginResultAsync(int userId, bool passwordValid)
{
    await using var connection = _database.CreateConnection();
    using var command = connection.CreateCommand();

    command.CommandText = "dbo.usp_RecordLoginResult";
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
    command.Parameters.Add("@PasswordValid", SqlDbType.Bit).Value = passwordValid;

    await connection.OpenAsync();

    var result = await command.ExecuteScalarAsync();
    return result is true;
}

    public async Task<bool> UpdatePasswordAsync(
        int userId, string passwordHash, string salt, int iterations)
    {
        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE dbo.Users
            SET MatKhauBam = @PasswordHash, Salt = @Salt, SoVongLap = @Iterations
            WHERE UserID = @UserID;
            """;

        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 64).Value = passwordHash;
        command.Parameters.Add("@Salt", SqlDbType.NVarChar, 32).Value = salt;
        command.Parameters.Add("@Iterations", SqlDbType.Int).Value = iterations;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() == 1;
    }
    public async Task<HealthShared.UserProfilePayload?> GetFullProfileAsync(int userId)
    {
        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT u.UserID, u.TenDangNhap, u.HoTen, u.Email,
                   hp.NgaySinh, hp.GioiTinh, hp.ChieuCaoCm, hp.CanNangKg,
                   hp.DiUng, hp.BenhNen, hp.GhiChu, hp.NgayCapNhat
            FROM dbo.Users u
            LEFT JOIN dbo.HealthProfiles hp ON u.UserID = hp.UserID
            WHERE u.UserID = @UserID;
            """;

        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new HealthShared.UserProfilePayload(
            UserId: reader.GetInt32(0),
            TenDangNhap: reader.GetString(1),
            HoTen: reader.GetString(2),
            Email: reader.IsDBNull(3) ? null : reader.GetString(3),
            NgaySinh: reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            GioiTinh: reader.IsDBNull(5) ? null : reader.GetString(5),
            ChieuCaoCm: reader.IsDBNull(6) ? null : reader.GetDecimal(6),
            CanNangKg: reader.IsDBNull(7) ? null : reader.GetDecimal(7),
            DiUng: reader.IsDBNull(8) ? null : reader.GetString(8),
            BenhNen: reader.IsDBNull(9) ? null : reader.GetString(9),
            GhiChu: reader.IsDBNull(10) ? null : reader.GetString(10),
            NgayCapNhat: reader.IsDBNull(11) ? null : reader.GetDateTime(11));
    }

    public async Task<bool> UpsertFullProfileAsync(
        int userId,
        string fullName,
        string? email,
        DateTime? ngaySinh,
        string? gioiTinh,
        decimal? chieuCaoCm,
        decimal? canNangKg,
        string? diUng,
        string? benhNen,
        string? ghiChu)
    {
        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        using (var userCmd = connection.CreateCommand())
        {
            userCmd.Transaction = (Microsoft.Data.SqlClient.SqlTransaction)transaction;
            userCmd.CommandText = """
                UPDATE dbo.Users
                SET HoTen = @FullName, Email = @Email
                WHERE UserID = @UserID;
                """;

            userCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
            userCmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
            userCmd.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value =
                (object?)email ?? DBNull.Value;

            if (await userCmd.ExecuteNonQueryAsync() != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        using (var profileCmd = connection.CreateCommand())
        {
            profileCmd.Transaction = (Microsoft.Data.SqlClient.SqlTransaction)transaction;
            profileCmd.CommandText = """
                IF EXISTS (SELECT 1 FROM dbo.HealthProfiles WHERE UserID = @UserID)
                BEGIN
                    UPDATE dbo.HealthProfiles
                    SET NgaySinh = @NgaySinh,
                        GioiTinh = @GioiTinh,
                        ChieuCaoCm = @ChieuCaoCm,
                        CanNangKg = @CanNangKg,
                        DiUng = @DiUng,
                        BenhNen = @BenhNen,
                        GhiChu = @GhiChu,
                        NgayCapNhat = SYSUTCDATETIME()
                    WHERE UserID = @UserID;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.HealthProfiles
                        (UserID, NgaySinh, GioiTinh, ChieuCaoCm, CanNangKg, DiUng, BenhNen, GhiChu)
                    VALUES
                        (@UserID, @NgaySinh, @GioiTinh, @ChieuCaoCm, @CanNangKg, @DiUng, @BenhNen, @GhiChu);
                END
                """;

            profileCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
            profileCmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value =
                ngaySinh.HasValue ? ngaySinh.Value.Date : DBNull.Value;
            profileCmd.Parameters.Add("@GioiTinh", SqlDbType.VarChar, 10).Value =
                (object?)gioiTinh ?? DBNull.Value;

            var heightParam = profileCmd.Parameters.Add("@ChieuCaoCm", SqlDbType.Decimal);
            heightParam.Precision = 5;
            heightParam.Scale = 2;
            heightParam.Value = (object?)chieuCaoCm ?? DBNull.Value;

            var weightParam = profileCmd.Parameters.Add("@CanNangKg", SqlDbType.Decimal);
            weightParam.Precision = 5;
            weightParam.Scale = 2;
            weightParam.Value = (object?)canNangKg ?? DBNull.Value;

            profileCmd.Parameters.Add("@DiUng", SqlDbType.NVarChar, 2000).Value =
                (object?)diUng ?? DBNull.Value;
            profileCmd.Parameters.Add("@BenhNen", SqlDbType.NVarChar, 2000).Value =
                (object?)benhNen ?? DBNull.Value;
            profileCmd.Parameters.Add("@GhiChu", SqlDbType.NVarChar, 2000).Value =
                (object?)ghiChu ?? DBNull.Value;

            await profileCmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return true;
    }

    public async Task<List<HealthShared.SearchHistoryItemPayload>> GetSearchHistoryAsync(int userId, int limit = 50)
    {
        int safeLimit = Math.Clamp(limit, 1, 200);
        var items = new List<HealthShared.SearchHistoryItemPayload>();

        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT TOP (@Limit)
                   h.SearchHistoryID, h.TuKhoa, h.DiseaseID, d.TenBenh, h.SoKetQua, h.ThoiGianTim
            FROM dbo.SearchHistory h
            LEFT JOIN dbo.Diseases d ON h.DiseaseID = d.DiseaseID
            WHERE h.UserID = @UserID
            ORDER BY h.ThoiGianTim DESC;
            """;

        command.Parameters.Add("@Limit", SqlDbType.Int).Value = safeLimit;
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new HealthShared.SearchHistoryItemPayload(
                SearchHistoryId: reader.GetInt64(0),
                TuKhoa: reader.GetString(1),
                DiseaseId: reader.IsDBNull(2) ? null : reader.GetInt32(2),
                TenBenh: reader.IsDBNull(3) ? null : reader.GetString(3),
                SoKetQua: reader.GetInt32(4),
                ThoiGianTim: reader.GetDateTime(5)));
        }

        return items;
    }

    public async Task<long> AddSearchHistoryAsync(
        int userId, string tuKhoa, int? diseaseId, int soKetQua, string? clientIp)
    {
        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
            DECLARE @ValidDiseaseID int = NULL;
            IF @DiseaseID IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Diseases WHERE DiseaseID = @DiseaseID)
                SET @ValidDiseaseID = @DiseaseID;

            INSERT INTO dbo.SearchHistory (UserID, TuKhoa, DiseaseID, SoKetQua, ClientIP)
            OUTPUT INSERTED.SearchHistoryID
            VALUES (@UserID, @TuKhoa, @ValidDiseaseID, @SoKetQua, @ClientIP);
            """;

        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 500).Value = tuKhoa;
        command.Parameters.Add("@DiseaseID", SqlDbType.Int).Value = (object?)diseaseId ?? DBNull.Value;
        command.Parameters.Add("@SoKetQua", SqlDbType.Int).Value = Math.Max(0, soKetQua);
        command.Parameters.Add("@ClientIP", SqlDbType.VarChar, 45).Value = (object?)clientIp ?? DBNull.Value;

        await connection.OpenAsync();
        return (long)(await command.ExecuteScalarAsync() ?? 0L);
    }

    public async Task<int> ClearSearchHistoryAsync(int userId)
    {
        await using var connection = _database.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM dbo.SearchHistory WHERE UserID = @UserID;";
        command.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }
}