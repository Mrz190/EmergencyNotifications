using API.Dto;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Controllers
{
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
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            await _emailService.SendEmailAsync(request.Email, request.Subject, request.Message);
            return Ok("Email sent successfully");
        }
    }
    public class EmailRequest
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
