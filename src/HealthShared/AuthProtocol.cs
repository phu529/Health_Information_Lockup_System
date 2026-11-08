namespace HealthShared;

public static class AuthCommands
{
    public const string Login = "LOGIN";
    public const string Register = "REGISTER";
}

public sealed record LoginPayload(
    string TenDangNhap,
    string MatKhau);

public sealed record RegisterPayload(
    string TenDangNhap,
    string MatKhau,
    string HoTen,
    string? Email);

public sealed record AuthSuccessPayload(string HoTen);