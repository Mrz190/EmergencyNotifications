using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class EmailController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IMailService _emailService;

        public EmailController(IMailService emailService, IMapper mapper)
        {
            _emailService = emailService;
            _mapper = mapper;
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendEmail([FromBody] EmailRequest request)
        {
            await _emailService.SendEmailAsync_(request.Email, request.Subject, request.Message);
            return Ok("Email sent successfully");
        }

        [HttpPost("send_mail")]
        public async Task<ActionResult> SendMailAsync(MailRequest mailRequest)
        {
            await _emailService.SendMailAsync(mailRequest);
            return Ok("Mail sent sucessfully");
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
    public class RequestEmail
    {
        public int Id { get; set; }
        public string Mail { get; set; }
    }





    public class Message
    {
        public string Subject { get; set; }
        public string MessageBody { get; set; }
    }
    public class Recipient
    {
        public int Id { get; set; }
        public string Mail { get; set; }
    }
    public class MailRequest
    {
        public Message MailMessage { get; set; } 
        public List<Recipient> Recipients { get; set; }
    }
}
