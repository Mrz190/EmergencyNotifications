using API.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using API.Controllers;
using API.Data;
using API.Dto;
using MongoDB.Driver;
using MongoDB.Bson;


namespace API.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IContactRepository _contactRepository;
        private readonly IMongoCollection<MailLogDto> _mailLogs;
        private const int MaxRetryAttempts = 3;
        private const int DelayMilliseconds = 2000;

        public MailService(IOptions<MailSettings> mailSettings, IContactRepository contactRepository, MongoDbContext mobgoDbContext)
        {
            _mailSettings = mailSettings.Value;
            _contactRepository = contactRepository;
            _mailLogs = mobgoDbContext.MailLogs;
        }

        public async Task<bool> SendMailAsync(MailRequest mailRequest)
        {
            var recipients = await GetRecipientsAsync(mailRequest.Recipients);

            if (!recipients.Any()) return false;

            using var smtp = CreateSmtpClient();
            
            foreach (var recipient in recipients)
            {
                var mailLog = await CreateMailLogAsync(mailRequest, recipient);
                bool success = await SendWithRetryAsync(smtp, mailRequest, recipient, mailLog);

                if(!success) return false;
            }
            return true;
        }

        private async Task<IEnumerable<Recipient>> GetRecipientsAsync(IEnumerable<Recipient> recipientRequests)
        {
            var recipients = await _contactRepository.GetContactsForMail(recipientRequests.ToList());
            return recipients;
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass),
                EnableSsl = true
            };
        }

        private async Task<MailLogDto> CreateMailLogAsync(MailRequest mailRequest, Recipient recipient)
        {
            var mailLog = new MailLogDto
            {
                MessageId = ObjectId.GenerateNewId().ToString(),
                Recipient = recipient.Mail,
                Subject = mailRequest.MailMessage.Subject,
                MessageBody = mailRequest.MailMessage.MessageBody,
                Status = "in process",
                CreatedAt = DateTime.UtcNow,
            };

            await _mailLogs.InsertOneAsync(mailLog);
            return mailLog;
        }

        private async Task<bool> SendWithRetryAsync(SmtpClient smtp, MailRequest mailRequest, Recipient recipient, MailLogDto mailLog)
        {
            var mailMessage = CreateMailMessage(mailRequest, recipient);

            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    await smtp.SendMailAsync(mailMessage);
                    await UpdateMailLogStatusAsync(mailLog, "sent");
                    return true;
                }
                catch (Exception ex)
                {
                    if (attempt == MaxRetryAttempts - 1)
                    {
                        await UpdateMailLogStatusAsync(mailLog, "not sent");
                        return false;
                    }

                    await Task.Delay(DelayMilliseconds * (int)Math.Pow(2, attempt));
                }
            }

            return false;
        }

        private MailMessage CreateMailMessage(MailRequest mailRequest, Recipient recipient)
        {
            var mailMessage = new MailMessage(_mailSettings.SmtpUser, recipient.Mail, mailRequest.MailMessage.Subject, mailRequest.MailMessage.MessageBody)
            {
                Priority = MailPriority.Normal,
                IsBodyHtml = true
            };

            mailMessage.Headers.Add("X-Priority", "3");
            mailMessage.Headers.Add("X-MSMail-Priority", "Normal");
            mailMessage.Headers.Add("Disposition-Notification-To", _mailSettings.SmtpUser);

            return mailMessage;
        }

        private async Task UpdateMailLogStatusAsync(MailLogDto mailLog, string status)
        {
            var filter = Builders<MailLogDto>.Filter.Eq(m => m.MessageId, mailLog.MessageId);
            var update = Builders<MailLogDto>.Update.Set(m => m.Status, status);
            await _mailLogs.UpdateOneAsync(filter, update);
        }
    }
}
