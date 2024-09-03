using API.Dto;
using API.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Cqrs.ContactService.Command
{
    public class AddContactCommand : IRequest<ActionResult<Contact>>
    {
        public NewContactDto newContactDto { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
    }
}
