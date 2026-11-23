using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace HealthServer;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly string? _host;
    private readonly int _port;
    private readonly bool _enableSsl;
    private readonly string? _username;
    private readonly string? _password;
    private readonly string? _fromEmail;
    private readonly string _fromName;

    public SmtpEmailSender()
    {
        using var configuration = new ConfigurationManager();
        configuration.SetBasePath(AppContext.BaseDirectory);
        configuration.AddJsonFile("appsettings.json", optional: false);

        var section = configuration.GetSection("Smtp");
        _host = section["Host"];
        _port = int.TryParse(section["Port"], out int p) ? p : 587;
        _enableSsl = !bool.TryParse(section["EnableSsl"], out bool ssl) || ssl;
        _username = section["Username"];
        _password = section["Password"];
        _fromEmail = section["FromEmail"] ?? _username;
        _fromName = section["FromName"] ?? "Hệ thống tra cứu sức khỏe";
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode)
    {
        if (string.IsNullOrWhiteSpace(_host) ||
            string.IsNullOrWhiteSpace(_username) ||
            string.IsNullOrWhiteSpace(_password) ||
            string.IsNullOrWhiteSpace(_fromEmail))
        {
            Console.WriteLine($"[OTP DEV FALLBACK] To: {toEmail} | OTP: {otpCode}");
            return;
        }

        string safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "Quý người dùng" : fullName.Trim());
        string safeOtp = WebUtility.HtmlEncode(otpCode.Trim());
        string bannerPath = Path.Combine(AppContext.BaseDirectory, "Assets", "email_banner.png");
        bool hasBanner = File.Exists(bannerPath);

        string illustrationHtml = hasBanner
            ? """
              <tr>
                <td align="center" style="padding:20px 28px 6px 28px;background-color:#FFFFFF;">
                  <img src="cid:health_banner" alt="Cộng đồng chăm sóc sức khỏe" width="524" style="display:block;width:100%;max-width:524px;height:auto;border:0;" />
                </td>
              </tr>
              """
            : string.Empty;

        string htmlBody = $$"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            </head>
            <body style="margin:0;padding:0;background-color:#F8FAFC;font-family:Arial,'Segoe UI',Roboto,Helvetica,sans-serif;color:#111827;-webkit-font-smoothing:antialiased;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#F8FAFC;padding:28px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:580px;background-color:#FFFFFF;border:1px solid #E2E8F0;border-top:3px solid #1F4B82;border-radius:3px;">
                      <!-- Header -->
                      <tr>
                        <td style="padding:18px 28px;border-bottom:1px solid #E2E8F0;">
                          <table role="presentation" cellspacing="0" cellpadding="0">
                            <tr>
                              <td style="width:22px;height:22px;background-color:#1F4B82;border-radius:2px;text-align:center;vertical-align:middle;font-family:Arial,sans-serif;font-size:16px;font-weight:700;color:#FFFFFF;line-height:22px;">+</td>
                              <td style="padding-left:10px;font-family:Arial,'Segoe UI',sans-serif;font-size:13px;font-weight:700;letter-spacing:0.6px;color:#1F4B82;">HỆ THỐNG TRA CỨU SỨC KHỎE</td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      {{illustrationHtml}}

                      <!-- Body -->
                      <tr>
                        <td style="padding:20px 28px 24px 28px;">
                          <h1 style="margin:0 0 14px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:20px;line-height:1.35;font-weight:700;color:#0F172A;">Mã xác nhận đặt lại mật khẩu</h1>

                          <p style="margin:0 0 10px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:14.5px;line-height:1.65;color:#111827;">
                            <strong>{{safeName}}</strong> thân mến,
                          </p>
                          <p style="margin:0 0 20px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:14px;line-height:1.65;color:#374151;">
                            Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sao chép mã xác nhận gồm 6 chữ số bên dưới và nhập vào cửa sổ <strong>Quên mật khẩu</strong> trên ứng dụng:
                          </p>

                          <!-- Clean Square OTP Box (Arial font: pure oval 0 without dot inside) -->
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#F8FAFC;border:1px solid #CBD5E1;border-radius:3px;margin:0 0 18px 0;">
                            <tr>
                              <td align="center" style="padding:18px 16px 6px 16px;font-family:Arial,'Segoe UI',sans-serif;font-size:11px;font-weight:700;letter-spacing:1px;color:#64748B;text-transform:uppercase;">
                                MÃ XÁC NHẬN (OTP)
                              </td>
                            </tr>
                            <tr>
                              <td align="center" style="padding:4px 16px 8px 16px;">
                                <span style="display:inline-block;font-family:Arial,Helvetica,'Segoe UI',sans-serif;font-size:34px;font-weight:700;letter-spacing:8px;color:#1F4B82;line-height:1.2;font-variant-numeric:tabular-nums;user-select:all;-webkit-user-select:all;">{{safeOtp}}</span>
                              </td>
                            </tr>
                            <tr>
                              <td align="center" style="padding:0 16px 16px 16px;font-family:Arial,'Segoe UI',sans-serif;font-size:12px;color:#64748B;">
                                Nhấp đúp (máy tính) hoặc nhấn giữ (điện thoại) vào dãy số trên để sao chép nhanh
                              </td>
                            </tr>
                          </table>

                          <!-- Validity -->
                          <p style="margin:0 0 16px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:13.5px;line-height:1.65;color:#374151;">
                            <strong>Hiệu lực:</strong> 15 phút (chỉ sử dụng 1 lần) &nbsp;&#183;&nbsp; <strong>Giới hạn:</strong> tối đa 5 lần nhập sai.
                          </p>

                          <!-- Security Note -->
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-left:2px solid #94A3B8;margin:0 0 22px 0;">
                            <tr>
                              <td style="padding:2px 0 2px 12px;font-family:Arial,'Segoe UI',sans-serif;font-size:13px;line-height:1.65;color:#4B5563;">
                                <strong style="color:#111827;">Lưu ý bảo mật:</strong> Tuyệt đối không chia sẻ mã này cho bất kỳ ai, kể cả người tự xưng là quản trị viên hay nhân viên hỗ trợ. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này — tài khoản của bạn vẫn hoàn toàn an toàn.
                              </td>
                            </tr>
                          </table>

                          <!-- Dignified Closing & Thank-you (Timo Bank inspired) -->
                          <p style="margin:0 0 16px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:14px;line-height:1.65;color:#111827;">
                            Cảm ơn Quý người dùng đã tin tưởng và sử dụng Hệ thống tra cứu sức khỏe!
                          </p>
                          <p style="margin:0 0 4px 0;font-family:Arial,'Segoe UI',sans-serif;font-size:14px;line-height:1.5;color:#374151;">
                            Trân trọng,
                          </p>
                          <p style="margin:0;font-family:Arial,'Segoe UI',sans-serif;font-size:14.5px;font-weight:700;line-height:1.5;color:#1F4B82;">
                            Hệ thống tra cứu sức khỏe
                          </p>
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="padding:14px 28px;border-top:1px solid #E2E8F0;background-color:#F8FAFC;text-align:center;">
                          <div style="font-family:Arial,'Segoe UI',sans-serif;font-size:12px;font-weight:700;color:#1F4B82;margin-bottom:2px;">Sức khỏe là tài sản quý giá nhất</div>
                          <div style="font-family:Arial,'Segoe UI',sans-serif;font-size:11.5px;color:#64748B;">Email tự động từ hệ thống · Vui lòng không trả lời thư này.</div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        using var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName, Encoding.UTF8),
            Subject = "Mã xác nhận đặt lại mật khẩu — Hệ thống tra cứu sức khỏe",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
        if (hasBanner)
        {
            var inlineBanner = new LinkedResource(bannerPath, MediaTypeNames.Image.Png)
            {
                ContentId = "health_banner",
                TransferEncoding = TransferEncoding.Base64
            };
            htmlView.LinkedResources.Add(inlineBanner);
        }
        message.AlternateViews.Add(htmlView);

        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _enableSsl,
            Credentials = new NetworkCredential(_username, _password)
        };

        await client.SendMailAsync(message);
        Console.WriteLine($"[SMTP] Đã gửi email HTML tối giản v5 tới {toEmail} (banner={hasBanner})");
    }
}
