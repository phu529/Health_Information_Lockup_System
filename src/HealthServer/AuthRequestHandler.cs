using System.Security.Cryptography;
using HealthShared;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace HealthServer;

internal sealed class AuthRequestHandler
{
    private readonly UserRepository _users;

    public AuthRequestHandler(UserRepository users)
    {
        _users = users;
    }

    public async Task<BaseResponse> LoginAsync(LoginPayload? payload)
    {
        if (payload is null ||
            string.IsNullOrWhiteSpace(payload.TenDangNhap) ||
            string.IsNullOrEmpty(payload.MatKhau))
        {
            return LoginResponse(ErrorCode.BadRequest, "Thiếu tên đăng nhập hoặc mật khẩu.");
        }

        var user = await _users.FindForLoginAsync(payload.TenDangNhap);

        if (user is null || !await VerifyAndRecordAsync(user, payload.MatKhau))
        {
            return LoginResponse(ErrorCode.Unauthorized, "Đăng nhập bị từ chối.");
        }

        var profile = await _users.GetProfileAsync(user.UserId);
        if (profile is null)
        {
            return LoginResponse(ErrorCode.InternalServerError, "Không đọc được hồ sơ.");
        }

        return LoginResponse(
            ErrorCode.Success,
            "Đăng nhập thành công.",
            ProtocolHelper.SerializeData(new AuthSuccessPayload(profile.FullName)));
    }

    private async Task<bool> VerifyAndRecordAsync(LoginUser user, string password)
    {
        var salt = Convert.FromBase64String(user.Salt);
        var storedHash = Convert.FromBase64String(user.PasswordHash);
        var enteredHash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, user.Iterations, HashAlgorithmName.SHA256, 32);

        bool matches = CryptographicOperations.FixedTimeEquals(enteredHash, storedHash);
        return await _users.RecordLoginResultAsync(user.UserId, matches);
    }

    private static BaseResponse LoginResponse(
        ErrorCode status, string message, string payload = "")
    {
        return new BaseResponse
        {
            Command = AuthCommands.Login,
            Status = status,
            Message = message,
            Payload = payload
        };
    }

    public async Task<BaseResponse> RegisterAsync(RegisterPayload? payload)
{
    var username = payload?.TenDangNhap?.Trim();
    var password = payload?.MatKhau;
    var fullName = payload?.HoTen?.Trim();
    var email = payload?.Email?.Trim();

    if (string.IsNullOrWhiteSpace(fullName) ||
        fullName.Length is < 2 or > 50 ||
        !fullName.All(c => char.IsLetter(c) || c == ' '))
    {
        return RegisterResponse(ErrorCode.BadRequest,
            "Họ tên cần 2–50 ký tự, gồm chữ cái và khoảng trắng.");
    }

    if (username is null || !Regex.IsMatch(username, @"\A[A-Za-z0-9_]{3,20}\z"))
    {
        return RegisterResponse(ErrorCode.BadRequest,
            "Tên đăng nhập cần 3–20 ký tự, gồm chữ không dấu, số hoặc dấu gạch dưới.");
    }

    if (password is null || password.Length is < 8 or > 64 ||
        !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
    {
        return RegisterResponse(ErrorCode.BadRequest,
            "Mật khẩu cần 8–64 ký tự, có ít nhất một chữ và một số.");
    }

    if (!string.IsNullOrEmpty(email) &&
        (email.Length > 100 || !Regex.IsMatch(email, @"\A[^@\s]+@[^@\s]+\.[^@\s]+\z")))
    {
        return RegisterResponse(ErrorCode.BadRequest, "Email phải có dạng ten@mien.com.");
    }

    var salt = RandomNumberGenerator.GetBytes(16);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        password, salt, 600_000, HashAlgorithmName.SHA256, 32);

    try
    {
        await _users.CreateUserAsync(
            username,
            Convert.ToBase64String(hash),
            Convert.ToBase64String(salt),
            fullName,
            string.IsNullOrEmpty(email) ? null : email);
    }
    catch (SqlException ex) when (ex.Number is 2601 or 2627)
    {
        return RegisterResponse(ErrorCode.Conflict,
            "Tên đăng nhập hoặc email đã được sử dụng.");
    }

    return RegisterResponse(
        ErrorCode.Success,
        "Đăng ký thành công.",
        ProtocolHelper.SerializeData(new AuthSuccessPayload(fullName)));
}

private static BaseResponse RegisterResponse(
    ErrorCode status, string message, string payload = "")
{
    return new BaseResponse
    {
        Command = AuthCommands.Register,
        Status = status,
        Message = message,
        Payload = payload
    };
}
}