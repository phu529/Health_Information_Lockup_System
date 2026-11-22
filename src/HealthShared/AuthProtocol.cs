namespace HealthShared;

public static class AuthCommands
{
    public const string Login = "LOGIN";
    public const string Register = "REGISTER";
    public const string ForgotPassword = "FORGOT_PWD";

    public const string RequestPasswordResetOtp = "FORGOT_PWD_REQUEST_OTP";
    public const string VerifyPasswordResetOtp = "FORGOT_PWD_VERIFY_OTP";
    public const string ResetPasswordWithToken = "FORGOT_PWD_RESET";
}

public sealed record LoginPayload(
    string TenDangNhap,
    string MatKhau);

public sealed record RegisterPayload(
    string TenDangNhap,
    string MatKhau,
    string HoTen,
    string? Email);

public sealed record ForgotPasswordPayload(
    string TenDangNhap,
    string Email,
    string MatKhauMoi);

public sealed record RequestPasswordResetOtpPayload(
    string Email);

public sealed record VerifyPasswordResetOtpPayload(
    string Email,
    string OtpCode);

public sealed record VerifyPasswordResetOtpResponsePayload(
    string ResetToken);

public sealed record ResetPasswordWithTokenPayload(
    string ResetToken,
    string MatKhauMoi);

public sealed record AuthSuccessPayload(string HoTen);