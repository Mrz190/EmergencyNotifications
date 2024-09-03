using API.Dto;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace API.Cqrs.ContactService.Command
{
    public class PatchContactCommand : IRequest<IActionResult>
    {
        public JsonPatchDocument<EditContactDto> PatchDoc { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public EditContactDto UpdatedContact { get; set; }
    }
}
