using API.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Cqrs.ContactService.Command
{
    public class PutContactCommand : IRequest<IActionResult>
    {
        public int Id { get; set; }
        public EditContactDto UpdatedContact { get; set; }
        public string UserName { get; set; }
    }
}
