using System.Net;
using System.Net.Mail;
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
        _fromName = section["FromName"] ?? "Health Information System";
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

        using var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = "Mã xác nhận đặt lại mật khẩu (hiệu lực 15 phút)",
            Body = $"""
                Xin chào {fullName},

                Mã OTP đặt lại mật khẩu của bạn là: {otpCode}
                Mã có hiệu lực trong 15 phút và tối đa 5 lần nhập sai.

                Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                """,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _enableSsl,
            Credentials = new NetworkCredential(_username, _password)
        };

        await client.SendMailAsync(message);
    }
}