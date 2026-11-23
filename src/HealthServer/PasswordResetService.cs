using System.Security.Cryptography;
using System.Text;

namespace HealthServer;

internal interface IEmailSender
{
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode);
}

internal sealed record OtpRequestResult(bool Success, string Message, bool RateLimited = false);

internal enum ResetPasswordOutcome
{
    Success,
    InvalidOrExpiredToken,
    SameAsOldPassword
}

internal sealed class PasswordResetService
{
    private const int OtpExpiryMinutes = 15;
    private const int ResetTokenExpiryMinutes = 10;

    private readonly UserRepository _userRepo;
    private readonly PasswordResetRepository _resetRepo;
    private readonly IEmailSender _emailSender;
    private readonly MailQueue? _mailQueue;
    private readonly ResetRateLimiter _rateLimiter;

    public PasswordResetService(
        UserRepository userRepo,
        PasswordResetRepository resetRepo,
        IEmailSender emailSender,
        MailQueue? mailQueue = null,
        ResetRateLimiter? rateLimiter = null)
    {
        _userRepo = userRepo;
        _resetRepo = resetRepo;
        _emailSender = emailSender;
        _mailQueue = mailQueue;
        _rateLimiter = rateLimiter ?? new ResetRateLimiter();
    }

    public async Task<OtpRequestResult> RequestResetOtpAsync(string email, string? tenDangNhap = null, string? clientIp = null)
    {
        string cleanEmail = email.Trim().ToLowerInvariant();
        string cleanUser = (tenDangNhap ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(cleanUser))
            return new OtpRequestResult(false, "Vui lòng nhập đầy đủ tên đăng nhập và email đã đăng ký.");

        var (choPhep, _, thongBaoGioiHan) = _rateLimiter.KiemTraVaGhiNhanGuiMa(cleanEmail, clientIp);
        if (!choPhep)
            return new OtpRequestResult(false, thongBaoGioiHan, RateLimited: true);

        var loginUser = await _userRepo.FindForLoginAsync(cleanUser);
        var profileByEmail = await _userRepo.FindActiveProfileByEmailAsync(cleanEmail);

        if (loginUser is null && profileByEmail is null)
            return new OtpRequestResult(false, "Không tìm thấy tài khoản và email này trong hệ thống. Vui lòng kiểm tra lại.");

        if (loginUser is not null)
        {
            if (!loginUser.IsActive)
                return new OtpRequestResult(false, "Tài khoản này hiện không còn hoạt động. Vui lòng liên hệ quản trị viên.");

            var profileByUser = await _userRepo.GetProfileAsync(loginUser.UserId);
            if (profileByUser is null ||
                string.IsNullOrWhiteSpace(profileByUser.Email) ||
                !string.Equals(profileByUser.Email.Trim(), cleanEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new OtpRequestResult(false, "Email không khớp với tên đăng nhập đã đăng ký. Vui lòng kiểm tra lại email.");
            }

            if (profileByUser.Email.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase))
                return new OtpRequestResult(false, "Tài khoản này chưa có địa chỉ email hợp lệ để nhận mã OTP.");

            string otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            byte[] otpHash = Sha256Bytes(otpCode);
            DateTime otpExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

            await _resetRepo.CreateOrReplaceRequestAsync(profileByUser.UserId, otpHash, otpExpiresAtUtc);

            if (_mailQueue is not null)
                await _mailQueue.EnqueueAsync(new MailJob(profileByUser.Email, profileByUser.FullName, otpCode));
            else
                await _emailSender.SendPasswordResetOtpAsync(profileByUser.Email, profileByUser.FullName, otpCode);

            return new OtpRequestResult(true, "Đã gửi mã xác nhận OTP 6 chữ số đến email của bạn.");
        }

        return new OtpRequestResult(false, "Tên đăng nhập không khớp với email đã đăng ký. Vui lòng kiểm tra lại tên đăng nhập.");
    }

    public async Task<(OtpVerifyResult Result, string? ResetToken)> VerifyOtpAsync(string email, string otpCode)
    {
        if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Trim().Length != 6)
            return (new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0), null);

        var profile = await _userRepo.FindActiveProfileByEmailAsync(email);
        if (profile is null)
            return (new OtpVerifyResult(OtpVerifyStatus.InvalidOrExpired, 0), null);

        string resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        byte[] otpHash = Sha256Bytes(otpCode.Trim());
        byte[] resetTokenHash = Sha256Bytes(resetToken);
        DateTime resetTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes);

        var result = await _resetRepo.VerifyOtpAndIssueResetTokenAsync(
            profile.UserId, otpHash, resetTokenHash, resetTokenExpiresAtUtc);

        if (result.Status == OtpVerifyStatus.TooManyAttempts)
            _rateLimiter.KhoaEmailTamThoi(email);

        return result.Status == OtpVerifyStatus.Success ? (result, resetToken) : (result, null);
    }

    public async Task<(ResetPasswordOutcome Outcome, int? UserId)> ResetPasswordWithCheckAsync(
        string resetToken,
        string newPassword,
        string? clientIp = null)
    {
        if (string.IsNullOrWhiteSpace(resetToken) || string.IsNullOrEmpty(newPassword))
            return (ResetPasswordOutcome.InvalidOrExpiredToken, null);

        byte[] resetTokenHash = Sha256Bytes(resetToken.Trim());
        var existing = await _resetRepo.GetPasswordInfoByResetTokenAsync(resetTokenHash);
        if (existing is null)
            return (ResetPasswordOutcome.InvalidOrExpiredToken, null);

        if (MatKhau.KiemTra(newPassword, existing.Value.Salt, existing.Value.PasswordHash, existing.Value.Iterations))
            return (ResetPasswordOutcome.SameAsOldPassword, null);

        var (saltBase64, hashBase64) = MatKhau.Tao(newPassword, MatKhau.VONG_LAP_MAC_DINH);
        int? userId = await _resetRepo.ConsumeResetTokenAndUpdatePasswordAsync(
            resetTokenHash, hashBase64, saltBase64, MatKhau.VONG_LAP_MAC_DINH, clientIp);

        return userId is null
            ? (ResetPasswordOutcome.InvalidOrExpiredToken, null)
            : (ResetPasswordOutcome.Success, userId);
    }

    private static byte[] Sha256Bytes(string input) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(input));
}
