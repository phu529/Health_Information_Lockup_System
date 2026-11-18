using System.Security.Cryptography;
using HealthShared;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using HealthShared.Validation;

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
            InputValidator.KiemTraDangNhap(payload.TenDangNhap, payload.MatKhau).Count > 0)
        {
            return LoginResponse(ErrorCode.BadRequest, "Thiếu tên đăng nhập hoặc mật khẩu.");
        }

        var user = await _users.FindForLoginAsync(payload.TenDangNhap.Trim());

        if(user?.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            int minutes = (int)Math.Ceiling((lockedUntil - DateTime.UtcNow).TotalMinutes);
            return LoginResponse(ErrorCode.Locked,
                $"Tài khoản đang bị khóa, thử lại sau {minutes} phút.");
        }

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

    var errors = InputValidator.KiemTraDangKy(
        fullName, username, password, password, email);
    if (errors.Count > 0)
        return RegisterResponse(ErrorCode.BadRequest, string.Join(" ", errors.Values));

    username = username!;

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

    public async Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordPayload? payload)
    {
        var username = payload?.TenDangNhap?.Trim();
        var email = payload?.Email?.Trim();
        var newPassword = payload?.MatKhauMoi;

        if (InputValidator.KiemTraQuenMatKhau(username, email, newPassword).Count > 0)
            return ForgotResponse(ErrorCode.BadRequest, "Thông tin nhập chưa hợp lệ.");

        const string VerifyFailed  = "Thông tin xác minh không đúng.";

        var user = await _users.FindForLoginAsync(username);
        if(user is null || !user.IsActive)
            return ForgotResponse(ErrorCode.Unauthorized, VerifyFailed);

        if(user.LockedUntil is { } until && until > DateTime.UtcNow)
            return ForgotResponse(ErrorCode.Locked, "Tài khoản đang bị khóa, thử lại sau.");

        var profile = await _users.GetProfileAsync(user.UserId);
        if(profile?.Email is null ||
            !string.Equals(profile.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            return ForgotResponse(ErrorCode.Unauthorized, VerifyFailed);
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            newPassword, salt, 600_000, HashAlgorithmName.SHA256, 32);

        bool updated = await _users.UpdatePasswordAsync(
            user.UserId, Convert.ToBase64String(hash), Convert.ToBase64String(salt), 600_000);

        if(!updated)
        {
            return ForgotResponse(ErrorCode.InternalServerError, "Không đổi được mật khẩu.");
        }

        return ForgotResponse(ErrorCode.Success, "Đổi mật khẩu thành công.");
    }

    private static BaseResponse ForgotResponse(ErrorCode status, string message)
    {
        return new BaseResponse
        {
            Command = AuthCommands.ForgotPassword,
            Status = status,
            Message = message,
            Payload = ""
        };
    }
}