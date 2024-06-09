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

        [HttpPost("send-mail")]
        public async Task<ActionResult> SendMailAsync(MailRequest mailRequest)
        {
            if (await _emailService.SendMailAsync(mailRequest) == false) return BadRequest("Something was wrong while sending message.");
            return Ok("Mail sent sucessfully");
        }
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
