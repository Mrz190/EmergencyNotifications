using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Cqrs.ContactService.Command
{
    public class DelContactCommand : IRequest<IActionResult>
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }
}
