using API.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using static IdentityServer4.Models.IdentityResources;

namespace API.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IContactRepository _contactRepository;

        public MailService(IOptions<MailSettings> mailSettings, IContactRepository contactRepository)
        {
            _mailSettings = mailSettings.Value;
            _contactRepository = contactRepository;
        }

        public async Task SendMailAsync(MailRequest mailRequest)
        {
            using var smtp = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass),
                EnableSsl = true
            };

            var recipientIds = mailRequest.Recipients.Select(r => r.Id).ToList();
            var recipients = await _contactRepository.GetContactsForMail(recipientIds);
                  
            foreach (var recipient in recipients)
            {
                var mailMessage = new MailMessage(_mailSettings.SmtpUser, recipient.Mail, mailRequest.MailMessage.Subject, mailRequest.MailMessage.MessageBody);
                mailMessage.Priority = MailPriority.Normal;
                mailMessage.IsBodyHtml = true;
                mailMessage.Headers.Add("X-Priority", "3");
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");
                await smtp.SendMailAsync(mailMessage);
            }
        }
    }
}
