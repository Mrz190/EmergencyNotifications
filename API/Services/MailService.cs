using API.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using API.Controllers;
using API.Data;
using API.Dto;
using MongoDB.Driver;


namespace API.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IContactRepository _contactRepository;
        private readonly IMongoCollection<MailLogDto> _mailLogs;

        public MailService(IOptions<MailSettings> mailSettings, IContactRepository contactRepository, MongoDbContext mobgoDbContext)
        {
            _mailSettings = mailSettings.Value;
            _contactRepository = contactRepository;
            _mailLogs = mobgoDbContext.MailLogs;
        }

        public async Task<bool> SendMailAsync(MailRequest mailRequest)
        {
            using var smtp = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass),
                EnableSsl = true
            };

            var recipientIds = mailRequest.Recipients.Select(r => r.Id).ToList();
            List<Recipient> recipients_list = mailRequest.Recipients.ToList();

            var recipients = await _contactRepository.GetContactsForMail(recipients_list);
            if (recipients.Count() == 0) return false;

            foreach (var recipient in recipients)
            {
                var mailMessage = new MailMessage(_mailSettings.SmtpUser, recipient.Mail, mailRequest.MailMessage.Subject, mailRequest.MailMessage.MessageBody);
                mailMessage.Priority = MailPriority.Normal;
                mailMessage.IsBodyHtml = true;
                mailMessage.Headers.Add("X-Priority", "3");
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

                var mailLog = new MailLogDto
                {
                    Recipient = recipient.Mail,
                    Subject = mailRequest.MailMessage.Subject,
                    MessageBody = mailRequest.MailMessage.MessageBody,
                    Status = "in proccess",
                    CreatedAt = DateTime.UtcNow
                };
                await _mailLogs.InsertOneAsync(mailLog);

                try
                {
                    await smtp.SendMailAsync(mailMessage);
                    var filter = Builders<MailLogDto>.Filter.Eq(m => m.MessageId, mailLog.MessageId);
                    var update = Builders<MailLogDto>.Update.Set(m => m.Status, "sended");
                    await _mailLogs.UpdateOneAsync(filter, update);
                }
                catch (Exception)
                {
                    var filter = Builders<MailLogDto>.Filter.Eq(m => m.MessageId, mailLog.MessageId);
                    var update = Builders<MailLogDto>.Update.Set(m => m.Status, "don't sended");
                    await _mailLogs.UpdateOneAsync(filter, update);
                    return false;
                }
            }

            return true;
        }
    }
}
