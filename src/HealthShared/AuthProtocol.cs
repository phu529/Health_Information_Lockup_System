namespace HealthShared;

public static class AuthCommands
{
    public const string Login = "LOGIN";
    public const string Register = "REGISTER";
    public const string ForgotPassword = "FORGOT_PWD";
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

public sealed record AuthSuccessPayload(string HoTen);