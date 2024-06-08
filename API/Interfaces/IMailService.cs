using API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IMailService
    {
        public Task SendEmailAsync_(string email, string subject, string message);
        public Task SendMailAsync(MailRequest mailRequest);
    }
}
