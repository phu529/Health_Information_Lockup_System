using System.Text.RegularExpressions;

namespace HealthShared.Validation;

/// <summary>
/// Luật kiểm tra dữ liệu Đăng ký / Đăng nhập / Quên mật khẩu.
/// Client dùng để báo lỗi phía client, Server dùng để kiểm tra lại.
/// Trả về: tên trường -> thông báo lỗi. Rỗng = hợp lệ.
/// </summary>
public static class InputValidator
{
    public const int HoTenMin = 2;
    public const int HoTenMax = 50;
    public const int TenDangNhapMin = 3;
    public const int TenDangNhapMax = 20;
    public const int MatKhauMin = 8;
    public const int MatKhauMax = 64;
    public const int EmailMax = 100;

    /// <summary>Tên trường dùng làm khóa khi kiểm tra lỗi</summary>
    public static class Truong
    {
        public const string HoTen = "HoTen";
        public const string TenDangNhap = "TenDangNhap";
        public const string MatKhau = "MatKhau";
        public const string XacNhanMatKhau = "XacNhanMatKhau";
        public const string Email = "Email";
        public const string OtpCode = "OtpCode";
    }

    private static readonly Regex RegexTenDangNhap =
        new(@"^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);

    private static readonly Regex RegexEmail =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static readonly Regex RegexOtp =
        new(@"^\d{6}$", RegexOptions.Compiled);

    public static Dictionary<string, string> KiemTraDangKy(
        string? hoTen, string? tenDangNhap, string? matKhau, string? xacNhanMatKhau, string? email)
    {
        hoTen = (hoTen ?? "").Trim();
        tenDangNhap = (tenDangNhap ?? "").Trim();
        email = (email ?? "").Trim();
        matKhau ??= "";
        xacNhanMatKhau ??= "";

        var loi = new Dictionary<string, string>();

        if (hoTen.Length == 0)
            loi[Truong.HoTen] = "Vui lòng nhập họ và tên.";
        else if (hoTen.Length < HoTenMin || hoTen.Length > HoTenMax ||
                 !hoTen.All(c => char.IsLetter(c) || c == ' '))
            loi[Truong.HoTen] = "Họ tên cần 2-50 ký tự, gồm chữ cái và khoảng trắng.";

        if (tenDangNhap.Length == 0)
            loi[Truong.TenDangNhap] = "Vui lòng nhập tên đăng nhập.";
        else if (!RegexTenDangNhap.IsMatch(tenDangNhap))
            loi[Truong.TenDangNhap] = "Tên đăng nhập 3-20 ký tự: chữ không dấu, số, dấu _.";

        if (matKhau.Length == 0)
            loi[Truong.MatKhau] = "Vui lòng nhập mật khẩu.";
        else if (matKhau.Length < MatKhauMin || matKhau.Length > MatKhauMax ||
                 !matKhau.Any(char.IsLetter) || !matKhau.Any(char.IsDigit))
            loi[Truong.MatKhau] = "Mật khẩu cần 8-64 ký tự, có ít nhất một chữ và một số.";

        if (xacNhanMatKhau.Length == 0)
            loi[Truong.XacNhanMatKhau] = "Vui lòng nhập lại mật khẩu.";
        else if (xacNhanMatKhau != matKhau)
            loi[Truong.XacNhanMatKhau] = "Hai ô mật khẩu không giống nhau.";

        if (email.Length == 0)
            loi[Truong.Email] = "Email không được để trống.";
        else if (email.Length > EmailMax)
            loi[Truong.Email] = "Email tối đa 100 ký tự.";
        else if (!RegexEmail.IsMatch(email))
            loi[Truong.Email] = "Email phải có dạng ten@mien.com.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraDangNhap(
        string? tenDangNhap, string? matKhau)
    {
        tenDangNhap = (tenDangNhap ?? "").Trim();
        matKhau ??= "";

        var loi = new Dictionary<string, string>();

        if (tenDangNhap.Length == 0)
            loi[Truong.TenDangNhap] = "Vui lòng nhập tên đăng nhập.";
        else if (tenDangNhap.Length > TenDangNhapMax)
            loi[Truong.TenDangNhap] = "Tên đăng nhập không hợp lệ.";

        if (matKhau.Length == 0)
            loi[Truong.MatKhau] = "Vui lòng nhập mật khẩu.";
        else if (matKhau.Length > MatKhauMax)
            loi[Truong.MatKhau] = "Mật khẩu không hợp lệ.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraQuenMatKhau(
        string? tenDangNhap, string? email, string? matKhauMoi)
    {
        var loi = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(tenDangNhap))
            loi[Truong.TenDangNhap] = "Vui lòng nhập tên đăng nhập.";

        if (string.IsNullOrWhiteSpace(email))
            loi[Truong.Email] = "Vui lòng nhập email đã đăng ký.";

        matKhauMoi ??= "";
        if (matKhauMoi.Length == 0)
            loi[Truong.MatKhau] = "Vui lòng nhập mật khẩu mới.";
        else if (matKhauMoi.Length < MatKhauMin || matKhauMoi.Length > MatKhauMax ||
                 !matKhauMoi.Any(char.IsLetter) || !matKhauMoi.Any(char.IsDigit))
            loi[Truong.MatKhau] = "Mật khẩu cần 8-64 ký tự, có ít nhất một chữ và một số.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraEmail(string? email)
    {
        var loi = new Dictionary<string, string>();
        var trimmed = (email ?? "").Trim();

        if (trimmed.Length == 0)
            loi[Truong.Email] = "Vui lòng nhập email đã đăng ký.";
        else if (trimmed.Length > EmailMax)
            loi[Truong.Email] = "Email tối đa 100 ký tự.";
        else if (!RegexEmail.IsMatch(trimmed))
            loi[Truong.Email] = "Email phải có dạng ten@mien.com.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraOtp(string? otpCode)
    {
        var loi = new Dictionary<string, string>();
        var trimmed = (otpCode ?? "").Trim();

        if (!RegexOtp.IsMatch(trimmed))
            loi[Truong.OtpCode] = "Mã OTP phải gồm đúng 6 chữ số.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraDatLaiMatKhau(string? matKhauMoi)
    {
        var loi = new Dictionary<string, string>();
        matKhauMoi ??= "";

        if (matKhauMoi.Length == 0)
            loi[Truong.MatKhau] = "Vui lòng nhập mật khẩu mới.";
        else if (matKhauMoi.Length < MatKhauMin || matKhauMoi.Length > MatKhauMax ||
                 !matKhauMoi.Any(char.IsLetter) || !matKhauMoi.Any(char.IsDigit))
            loi[Truong.MatKhau] = "Mật khẩu cần 8-64 ký tự, có ít nhất một chữ và một số.";

        return loi;
    }

    public static Dictionary<string, string> KiemTraResetToken(string? resetToken)
    {
        var loi = new Dictionary<string, string>();
        var trimmed = (resetToken ?? "").Trim();
        if (trimmed.Length != 64 || !trimmed.All(Uri.IsHexDigit))
            loi["ResetToken"] = "Phiên đặt lại mật khẩu không hợp lệ.";
        return loi;
    }
}
