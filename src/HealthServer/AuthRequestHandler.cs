using HealthServer.Network;
using HealthShared;
using HealthShared.Validation;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace HealthServer;

internal sealed class AuthRequestHandler
{
    private readonly UserRepository _users;
    private readonly Action<int>? _thuHoiPhien;
    private readonly PasswordResetService? _passwordReset;

    public AuthRequestHandler(
        UserRepository users,
        Action<int>? thuHoiPhien = null,
        PasswordResetService? passwordReset = null)
    {
        _users = users;
        _thuHoiPhien = thuHoiPhien;
        _passwordReset = passwordReset;
    }

    public async Task<BaseResponse> LoginAsync(LoginPayload? payload, ClientSession? session = null)
    {
        if (payload is null ||
            InputValidator.KiemTraDangNhap(payload.TenDangNhap, payload.MatKhau).Count > 0)
        {
            return LoginResponse(ErrorCode.BadRequest, "Thiếu tên đăng nhập hoặc mật khẩu.");
        }

        var user = await _users.FindForLoginAsync(payload.TenDangNhap.Trim());

        if (user?.LockedUntil is { } lockedUntil && lockedUntil > DateTime.UtcNow)
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

        // Thu hồi toàn bộ phiên cũ đang đăng nhập bằng tài khoản này trên các máy khác
        _thuHoiPhien?.Invoke(user.UserId);

        // Gắn UserId vào phiên hiện tại để sau này có thể thu hồi nếu đổi mật khẩu
        session?.GanNguoiDung(user.UserId);

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
        fullName = fullName!;
        password = password!;
        email = email!;

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
                email);
        }
        catch (ArgumentException ex)
        {
            return RegisterResponse(ErrorCode.BadRequest, ex.Message);
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

    public async Task<BaseResponse> RequestPasswordResetOtpAsync(RequestPasswordResetOtpPayload? payload)
    {
        if (_passwordReset is null)
            return Response(AuthCommands.RequestPasswordResetOtp, ErrorCode.InternalServerError, "Dịch vụ quên mật khẩu chưa được cấu hình.");

        var errors = InputValidator.KiemTraEmail(payload?.Email);
        if (errors.Count > 0)
            return Response(AuthCommands.RequestPasswordResetOtp, ErrorCode.BadRequest, string.Join(" ", errors.Values));

        var reqResult = await _passwordReset.RequestResetOtpAsync(payload!.Email.Trim(), payload.TenDangNhap);
        if (!reqResult.Success)
        {
            return Response(
                AuthCommands.RequestPasswordResetOtp,
                ErrorCode.Unauthorized,
                reqResult.Message);
        }

        return Response(
            AuthCommands.RequestPasswordResetOtp,
            ErrorCode.Success,
            reqResult.Message);
    }

    public async Task<BaseResponse> VerifyPasswordResetOtpAsync(VerifyPasswordResetOtpPayload? payload)
    {
        if (_passwordReset is null)
            return Response(AuthCommands.VerifyPasswordResetOtp, ErrorCode.InternalServerError, "Dịch vụ quên mật khẩu chưa được cấu hình.");

        var emailErrors = InputValidator.KiemTraEmail(payload?.Email);
        var otpErrors = InputValidator.KiemTraOtp(payload?.OtpCode);
        if (emailErrors.Count > 0 || otpErrors.Count > 0)
        {
            var allErrors = emailErrors.Values.Concat(otpErrors.Values);
            return Response(AuthCommands.VerifyPasswordResetOtp, ErrorCode.BadRequest, string.Join(" ", allErrors));
        }

        var (result, resetToken) = await _passwordReset.VerifyOtpAsync(payload!.Email.Trim(), payload.OtpCode.Trim());

        return result.Status switch
        {
            OtpVerifyStatus.Success when resetToken is not null => Response(
                AuthCommands.VerifyPasswordResetOtp,
                ErrorCode.Success,
                "Xác thực OTP thành công.",
                ProtocolHelper.SerializeData(new VerifyPasswordResetOtpResponsePayload(resetToken))),

            OtpVerifyStatus.TooManyAttempts => Response(
                AuthCommands.VerifyPasswordResetOtp,
                ErrorCode.Locked,
                "Bạn đã nhập sai OTP quá 5 lần. Vui lòng yêu cầu mã OTP mới."),

            _ => Response(
                AuthCommands.VerifyPasswordResetOtp,
                ErrorCode.Unauthorized,
                result.RemainingAttempts > 0
                    ? $"Mã OTP không đúng hoặc đã hết hạn. Còn {result.RemainingAttempts} lần thử."
                    : "Mã OTP không đúng hoặc đã hết hạn.")
        };
    }

    public async Task<BaseResponse> ResetPasswordWithTokenAsync(ResetPasswordWithTokenPayload? payload)
    {
        if (_passwordReset is null)
            return Response(AuthCommands.ResetPasswordWithToken, ErrorCode.InternalServerError, "Dịch vụ quên mật khẩu chưa được cấu hình.");

        if (string.IsNullOrWhiteSpace(payload?.ResetToken))
            return Response(AuthCommands.ResetPasswordWithToken, ErrorCode.BadRequest, "Thiếu mã xác thực đặt lại mật khẩu.");

        var pwdErrors = InputValidator.KiemTraDatLaiMatKhau(payload.MatKhauMoi);
        if (pwdErrors.Count > 0)
            return Response(AuthCommands.ResetPasswordWithToken, ErrorCode.BadRequest, string.Join(" ", pwdErrors.Values));

        int? userId = await _passwordReset.ResetPasswordAsync(payload.ResetToken, payload.MatKhauMoi);
        if (userId is null)
        {
            return Response(
                AuthCommands.ResetPasswordWithToken,
                ErrorCode.Unauthorized,
                "Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        _thuHoiPhien?.Invoke(userId.Value);

        return Response(
            AuthCommands.ResetPasswordWithToken,
            ErrorCode.Success,
            "Đặt lại mật khẩu thành công.");
    }

    public async Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordPayload? payload)
    {
        var username = payload?.TenDangNhap?.Trim();
        var email = payload?.Email?.Trim();
        var newPassword = payload?.MatKhauMoi;

        if (InputValidator.KiemTraQuenMatKhau(username, email, newPassword).Count > 0)
            return ForgotResponse(ErrorCode.BadRequest, "Thông tin nhập chưa hợp lệ.");

        const string VerifyFailed = "Thông tin xác minh không đúng.";

        var user = await _users.FindForLoginAsync(username!);
        if (user is null || !user.IsActive)
            return ForgotResponse(ErrorCode.Unauthorized, VerifyFailed);

        if (user.LockedUntil is { } until && until > DateTime.UtcNow)
            return ForgotResponse(ErrorCode.Locked, "Tài khoản đang bị khóa, thử lại sau.");

        var profile = await _users.GetProfileAsync(user.UserId);
        if (profile?.Email is null ||
            !string.Equals(profile.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            return ForgotResponse(ErrorCode.Unauthorized, VerifyFailed);
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            newPassword!, salt, 600_000, HashAlgorithmName.SHA256, 32);

        bool updated = await _users.UpdatePasswordAsync(
            user.UserId, Convert.ToBase64String(hash), Convert.ToBase64String(salt), 600_000);

        if (!updated)
        {
            return ForgotResponse(ErrorCode.InternalServerError, "Không đổi được mật khẩu.");
        }

        _thuHoiPhien?.Invoke(user.UserId);

        return ForgotResponse(ErrorCode.Success, "Đổi mật khẩu thành công.");
    }

    private static BaseResponse ForgotResponse(ErrorCode status, string message)
        => Response(AuthCommands.ForgotPassword, status, message);

    private static BaseResponse Response(
        string command, ErrorCode status, string message, string payload = "")
    {
        return new BaseResponse
        {
            Command = command,
            Status = status,
            Message = message,
            Payload = payload
        };
    }
}