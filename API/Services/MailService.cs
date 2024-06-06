using API.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace API.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using var smtp = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(_mailSettings.SmtpUser, email, subject, message);
            mailMessage.Priority = MailPriority.Normal;
            mailMessage.IsBodyHtml = true;
            mailMessage.Headers.Add("X-Priority", "3");
            mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

            await smtp.SendMailAsync(mailMessage);
        }
    }
}
