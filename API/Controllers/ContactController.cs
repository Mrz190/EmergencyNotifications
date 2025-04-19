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
        private readonly ILogger<ContactController> _logger;

        public ContactController(IMediator mediator, ILogger<ContactController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Helper method to retrieve user info
        private (string UserName, string UserEmail, string UserPhone) GetUserClaims()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userPhone = User.FindFirst("Phone")?.Value; // Keep the full phone number

            return (userName, userEmail, userPhone);
        }

        // Add contact (POST)
        [HttpPost("add-contact")]
        public async Task<ActionResult<Contact>> AddContact(NewContactDto contactDto)
        {
            var (userName, userEmail, userPhone) = GetUserClaims();
            contactDto.CreatedBy = userName;

            try
            {
                var command = new AddContactCommand
                {
                    newContactDto = contactDto,
                    UserName = userName,
                    UserEmail = userEmail,
                    UserPhone = userPhone
                };

                var result = await _mediator.Send(command);

                if (result.Result is BadRequestObjectResult)
                {
                    return BadRequest(result.Value);
                }

                if (result.Result is ConflictObjectResult)
                {
                    return Conflict(result.Value);
                }

                if (result.Result is NotFoundObjectResult)
                {
                    return NotFound(result.Value);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact");
                return StatusCode(500, "Internal server error");
            }
        }

        // Get contacts list (GET)
        [HttpGet("contacts-list")]
        public async Task<ActionResult<IEnumerable<GetContactsDto>>> GetMyContacts()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var query = new GetContactQuery { UserName = userName };

            var contacts = await _mediator.Send(query);
            return Ok(contacts);
        }

        // Find contact by name (GET)
        [HttpGet("find-contact")]
        public async Task<ActionResult<IEnumerable<GetContactsDto>>> GetContact(string name)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            var query = new FindContactQuery { UserName = name, ContactCreator = contactCreator };

            var result = await _mediator.Send(query);
            return result.Any() ? Ok(result) : NotFound("No contacts found");
        }

        // Update contact (PUT)
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

            try
            {
                var result = await _mediator.Send(command);

                if (result != null)
                    return Ok("Contact updated successfully");

                return NotFound("Contact not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact");
                return StatusCode(500, "Internal server error");
            }
        }

        // Patch contact (PATCH)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchContact(int id, [FromBody] JsonPatchDocument<EditContactDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest("Invalid patch document.");

            var existingContactDto = await _mediator.Send(new GetContactByIdQuery(id));
            if (existingContactDto == null)
                return NotFound();

            var contactToPatch = new EditContactDto
            {
                Id = existingContactDto.Id,
                Name = existingContactDto.Name,
                Email = existingContactDto.Email
            };

            patchDoc.ApplyTo(contactToPatch);
            if (!TryValidateModel(contactToPatch))
                return ValidationProblem(ModelState);

            var command = new PatchContactCommand
            {
                Id = id,
                UpdatedContact = contactToPatch,
                PatchDoc = patchDoc
            };

            var result = await _mediator.Send(command);

            // Assuming result is an object that could be null or an indicator of success
            if (result != null)
                return Ok("Contact patched successfully");

            return NotFound("Contact not found");
        }

        // Delete contact (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            var command = new DelContactCommand
            {
                UserName = contactCreator,
                Id = id
            };

            try
            {
                var result = await _mediator.Send(command);

                if (result != null)
                    return Ok("Contact deleted successfully");

                return NotFound("Contact not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
