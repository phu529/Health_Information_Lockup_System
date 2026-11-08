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
}