using API.Cqrs.ContactService.Command;
using API.Cqrs.ContactService.Query;
using API.Dto;
using API.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class ContactController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ContactController(IMediator mediator)
        {
            _mediator = mediator;
        }
 
        [HttpPost("add-contact")]
        public async Task<ActionResult<Contact>> AddContact(NewContactDto contactDto)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userPhone = User.FindFirst("Phone")?.Value?.Remove(0, 7);

            contactDto.CreatedBy = userName;

            var command = new AddContactCommand
            {
                newContactDto = contactDto,
                UserName = userName,
                UserEmail = userEmail,
                UserPhone = userPhone
            };

            return await _mediator.Send(command);
        }

        [HttpGet("contacts-list")]
        public async Task<IEnumerable<GetContactsDto>> GetMyContacts()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);

            var query = new GetContactQuery
            {
                UserName = userName
            };

            return await _mediator.Send(query);
        }

        [HttpGet("find-contact")]
        public async Task<IEnumerable<GetContactsDto>> GetContact(string name)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            var query = new FindContactQuery
            {
                UserName = name,
                ContactCreator = contactCreator
            };

            return await _mediator.Send(query);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContact(int id, EditContactDto updatedContact)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            var command = new PutContactCommand
            {
                UserName = contactCreator,
                Id = id,
                UpdatedContact = updatedContact
            };

            return await _mediator.Send(command);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchContact(int id, [FromBody] JsonPatchDocument<EditContactDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Invalid patch document.");

            // Получение существующего контакта (можно использовать другой запрос или метод)
            var existingContactDto = await _mediator.Send(new GetContactByIdQuery(id));
            if (existingContactDto == null)
                return NotFound();

            // Применение патч-документа
            var contactToPatch = new EditContactDto
            {
                Id = existingContactDto.Id,
                Name = existingContactDto.Name,
                Email = existingContactDto.Email,
                // Другие свойства
            };

            patchDoc.ApplyTo(contactToPatch);

            // Валидация
            if (!TryValidateModel(contactToPatch))
            {
                return ValidationProblem(ModelState);
            }

            var command = new PatchContactCommand
            {
                Id = id,
                UpdatedContact = contactToPatch,
                PatchDoc = patchDoc
            };

            var result = await _mediator.Send(command);
            return result;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            var command = new DelContactCommand
            {
                UserName = contactCreator,
                Id = id
            };

            return await _mediator.Send(command);
        }
    }
}