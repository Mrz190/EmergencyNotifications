using API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IMailService
    {
        public Task<bool> SendMailAsync(MailRequest mailRequest);
    }
}
