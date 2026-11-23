using System.Collections.Concurrent;

namespace HealthServer;

/// <summary>
/// Chống spam gửi OTP và khóa tạm 15 phút khi nhập sai OTP đủ 5 lần.
/// Quy tắc: cách nhau >= 60s/email; <= 5 lần/giờ/email; <= 20 lần/giờ/IP.
/// </summary>
internal sealed class ResetRateLimiter
{
    private const int CooldownGiay = 60;
    private const int ToiDaEmailMoiGio = 5;
    private const int ToiDaIpMoiGio = 20;
    private const int KhoaSaiOtpPhut = 15;

    private readonly ConcurrentDictionary<string, List<DateTime>> _lichSuEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<DateTime>> _lichSuIp = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _khoaEmailDen = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _khoa = new();

    public (bool ChoPhep, int RetryAfterSeconds, string ThongBao) KiemTraVaGhiNhanGuiMa(string email, string? clientIp)
    {
        string keyEmail = email.Trim().ToLowerInvariant();
        string keyIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim();
        DateTime now = DateTime.UtcNow;

        lock (_khoa)
        {
            if (_khoaEmailDen.TryGetValue(keyEmail, out DateTime khoaDen))
            {
                if (khoaDen > now)
                {
                    int giay = Math.Max(1, (int)Math.Ceiling((khoaDen - now).TotalSeconds));
                    int phut = Math.Max(1, (int)Math.Ceiling(giay / 60.0));
                    return (false, giay, $"Email này đang tạm khóa xin mã do nhập sai OTP quá 5 lần. Vui lòng thử lại sau {phut} phút.");
                }
                _khoaEmailDen.TryRemove(keyEmail, out _);
            }

            var dsEmail = _lichSuEmail.GetOrAdd(keyEmail, _ => new List<DateTime>());
            dsEmail.RemoveAll(t => (now - t).TotalHours >= 1);

            if (dsEmail.Count > 0)
            {
                double giayTuLanCuoi = (now - dsEmail[^1]).TotalSeconds;
                if (giayTuLanCuoi < CooldownGiay)
                {
                    int cho = Math.Max(1, (int)Math.Ceiling(CooldownGiay - giayTuLanCuoi));
                    return (false, cho, $"Vui lòng chờ {cho} giây trước khi yêu cầu gửi lại mã OTP.");
                }
            }

            if (dsEmail.Count >= ToiDaEmailMoiGio)
            {
                int cho = Math.Max(1, (int)Math.Ceiling(3600 - (now - dsEmail[0]).TotalSeconds));
                return (false, cho, "Bạn đã yêu cầu gửi mã quá 5 lần trong 1 giờ cho email này. Vui lòng thử lại sau.");
            }

            var dsIp = _lichSuIp.GetOrAdd(keyIp, _ => new List<DateTime>());
            dsIp.RemoveAll(t => (now - t).TotalHours >= 1);
            if (dsIp.Count >= ToiDaIpMoiGio)
            {
                int cho = Math.Max(1, (int)Math.Ceiling(3600 - (now - dsIp[0]).TotalSeconds));
                return (false, cho, "Địa chỉ mạng của bạn đã gửi quá nhiều yêu cầu đặt lại mật khẩu. Vui lòng thử lại sau.");
            }

            dsEmail.Add(now);
            dsIp.Add(now);
            return (true, 0, string.Empty);
        }
    }

    public void KhoaEmailTamThoi(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return;
        string keyEmail = email.Trim().ToLowerInvariant();
        _khoaEmailDen[keyEmail] = DateTime.UtcNow.AddMinutes(KhoaSaiOtpPhut);
    }
}
