using System.Security.Cryptography;
using System.Text;

namespace HealthServer;

internal interface IEmailSender
{
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode);
}

internal sealed record OtpRequestResult(bool Success, string Message);

internal sealed class PasswordResetService
{
    private const int OtpExpiryMinutes = 15;
    private const int ResetTokenExpiryMinutes = 10;
    private const int Pbkdf2Iterations = 600_000;

    private readonly UserRepository _userRepo;
    private readonly PasswordResetRepository _resetRepo;
    private readonly IEmailSender _emailSender;

    public PasswordResetService(
        UserRepository userRepo,
        PasswordResetRepository resetRepo,
        IEmailSender emailSender)
    {
        _userRepo = userRepo;
        _resetRepo = resetRepo;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Bước 1: Kiểm tra chặt chẽ cặp Tên đăng nhập + Email trước khi sinh và gửi OTP 6 số.
    /// Trả cảnh báo rõ ràng để người dùng kiểm tra lại ngay tại Bước 1 (không cho qua Bước 2 nếu sai).
    /// </summary>
    public async Task<OtpRequestResult> RequestResetOtpAsync(string email, string? tenDangNhap = null)
    {
        string cleanEmail = email.Trim().ToLowerInvariant();
        string cleanUser = (tenDangNhap ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(cleanUser))
        {
            return new OtpRequestResult(false, "Vui lòng nhập đầy đủ tên đăng nhập và email đã đăng ký.");
        }

        // 1) Tìm theo Tên đăng nhập và theo Email song song để báo lỗi chính xác từng tình huống
        var loginUser = await _userRepo.FindForLoginAsync(cleanUser);
        var profileByEmail = await _userRepo.FindActiveProfileByEmailAsync(cleanEmail);

        if (loginUser is null && profileByEmail is null)
        {
            return new OtpRequestResult(
                false,
                "Không tìm thấy tài khoản và email này trong hệ thống. Vui lòng kiểm tra lại.");
        }

        if (loginUser is not null)
        {
            if (!loginUser.IsActive)
            {
                return new OtpRequestResult(
                    false,
                    "Tài khoản này hiện không còn hoạt động. Vui lòng liên hệ quản trị viên.");
            }

            var profileByUser = await _userRepo.GetProfileAsync(loginUser.UserId);
            if (profileByUser is null ||
                string.IsNullOrWhiteSpace(profileByUser.Email) ||
                !string.Equals(profileByUser.Email.Trim(), cleanEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new OtpRequestResult(
                    false,
                    "Email không khớp với tên đăng nhập đã đăng ký. Vui lòng kiểm tra lại email.");
            }

            // Khớp hoàn toàn cả Tên đăng nhập và Email!
            if (profileByUser.Email.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase))
            {
                return new OtpRequestResult(
                    false,
                    "Tài khoản này chưa có địa chỉ email hợp lệ để nhận mã OTP.");
            }

            string otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            byte[] otpHash = Sha256Bytes(otpCode);
            DateTime otpExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

            await _resetRepo.CreateOrReplaceRequestAsync(profileByUser.UserId, otpHash, otpExpiresAtUtc);
            await _emailSender.SendPasswordResetOtpAsync(profileByUser.Email, profileByUser.FullName, otpCode);

            return new OtpRequestResult(
                true,
                "Đã gửi mã xác nhận OTP 6 chữ số đến email của bạn.");
        }

        // Trường hợp còn lại: Email có tồn tại trong hệ thống nhưng Tên đăng nhập KHÔNG khớp với email đó
        return new OtpRequestResult(
            false,
            "Tên đăng nhập không khớp với email đã đăng ký. Vui lòng kiểm tra lại tên đăng nhập.");
    }

    /// <summary>
    /// Bước 2: Xác thực OTP 6 số. Thành công trả về resetToken (plaintext) hạn 10 phút.
    /// </summary>
    public async Task<(OtpVerifyResult Result, string? ResetToken)> VerifyOtpAsync(
        string email,
        string otpCode)
    {
        if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Trim().Length != 6)
        {
            return (new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0), null);
        }

        var profile = await _userRepo.FindActiveProfileByEmailAsync(email);
        if (profile is null)
        {
            return (new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0), null);
        }

        string resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        byte[] otpHash = Sha256Bytes(otpCode.Trim());
        byte[] resetTokenHash = Sha256Bytes(resetToken);
        DateTime resetTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes);

        var result = await _resetRepo.VerifyOtpAndIssueResetTokenAsync(
            profile.UserId,
            otpHash,
            resetTokenHash,
            resetTokenExpiresAtUtc);

        return result.Status == OtpVerifyStatus.Success
            ? (result, resetToken)
            : (result, null);
    }

    /// <summary>
    /// Bước 3: Dùng resetToken 1 lần để đặt lại mật khẩu mới (PBKDF2-HMAC-SHA256, 600.000 vòng, Base64).
    /// Trả về UserID nếu thành công để thu hồi toàn bộ phiên đăng nhập cũ.
    /// </summary>
    public async Task<int?> ResetPasswordAsync(string resetToken, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(resetToken) || string.IsNullOrEmpty(newPassword))
        {
            return null;
        }

        byte[] resetTokenHash = Sha256Bytes(resetToken.Trim());

        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
        byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            newPassword,
            saltBytes,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            32);

        string saltBase64 = Convert.ToBase64String(saltBytes);
        string hashBase64 = Convert.ToBase64String(hashBytes);

        return await _resetRepo.ConsumeResetTokenAndUpdatePasswordAsync(
            resetTokenHash,
            hashBase64,
            saltBase64,
            Pbkdf2Iterations);
    }

    private static byte[] Sha256Bytes(string input)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(input));
    }
}
