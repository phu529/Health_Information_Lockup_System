using System.Security.Cryptography;
using System.Text;

namespace HealthServer;

internal interface IEmailSender
{
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode);
}

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
    /// Bước 1: Yêu cầu gửi OTP 6 số qua email (nếu email thuộc tài khoản IsActive = 1).
    /// Trả về chung một thông báo trung tính để tránh lộ thông tin tài khoản.
    /// </summary>
    public async Task RequestResetOtpAsync(string email)
    {
        var profile = await _userRepo.FindActiveProfileByEmailAsync(email);
        if (profile is null || string.IsNullOrWhiteSpace(profile.Email))
        {
            return;
        }

        string otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        byte[] otpHash = Sha256Bytes(otpCode);
        DateTime otpExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

        await _resetRepo.CreateOrReplaceRequestAsync(profile.UserId, otpHash, otpExpiresAtUtc);
        await _emailSender.SendPasswordResetOtpAsync(profile.Email, profile.FullName, otpCode);
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