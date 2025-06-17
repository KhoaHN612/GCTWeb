using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options; // Cho IOptions
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using GCTWeb.Models.Configuration;
using Microsoft.Extensions.Logging; 

namespace GCTWeb.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> emailSettingsOptions, ILogger<SmtpEmailSender> logger)
        {
            _emailSettings = emailSettingsOptions.Value ?? throw new ArgumentNullException(nameof(emailSettingsOptions), "EmailSettings cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (string.IsNullOrEmpty(_emailSettings.SmtpServer) ||
                string.IsNullOrEmpty(_emailSettings.SmtpUsername) ||
                string.IsNullOrEmpty(_emailSettings.SmtpPassword) ||
                string.IsNullOrEmpty(_emailSettings.FromEmail))
            {
                _logger.LogWarning("SMTP Email settings are not fully configured. Email sending might fail or be skipped.");
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_emailSettings.SmtpServer) || string.IsNullOrEmpty(_emailSettings.FromEmail))
            {
                _logger.LogWarning("SMTP server or FromEmail not configured. Skipping email to {Email} with subject {Subject}.", email, subject);
                return; // Không gửi nếu cấu hình thiếu
            }

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName ?? _emailSettings.FromEmail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(new MailAddress(email));

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                    client.EnableSsl = _emailSettings.EnableSsl;
                    // client.Timeout = 20000; // 20 giây, tùy chọn

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent to {Email} with subject {Subject}.", email, subject);
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SmtpException occurred while sending email to {Email}. StatusCode: {StatusCode}", email, smtpEx.StatusCode);
                // Không throw lại lỗi để không làm dừng quy trình chính (ví dụ đăng ký)
                // Bạn có thể quyết định throw nếu việc gửi email là cực kỳ quan trọng
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error occurred while sending email to {Email} with subject {Subject}.", email, subject);
            }
        }
    }
}