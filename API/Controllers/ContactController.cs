using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize]
    public class ContactController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        public ContactController(IMapper mapper, IUnitOfWork unitOfWork, IContactRepository contactRepository)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _contactRepository = contactRepository;
        }

        [HttpPost("add-contact")]
        public async Task<ActionResult<Contact>> AddContact(NewContactDto contactDto)
        {
            var contactCreatorName = User.FindFirstValue(ClaimTypes.Name);
            
            if (contactDto.Phone.Any(c => !char.IsDigit(c) && c != '+')) return BadRequest("Phone can contains only digits.");
            if (!isEmailValid(contactDto.Email)) return BadRequest("Incorrect email.");

            if (await _contactRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, contactCreatorName)) return NotFound("Contact with this phone already exists.");
            if (await _contactRepository.UniqueContactEmailExists(contactDto.Name, contactDto.Email, contactCreatorName)) return NotFound("Contact with this email already exists.");

            var contactCreatorEmail = User.FindFirst(ClaimTypes.Email).ToString();
            var contactCreatorPhone_def = User.FindFirst("Phone").ToString();
            string contactCreatorPhone = contactCreatorPhone_def.Remove(0, 7);
            contactDto.CreatedBy = contactCreatorName;

            if (contactCreatorEmail == contactDto.Email) return BadRequest("Contact email same as your.");
            if (contactCreatorPhone == contactDto.Phone) return BadRequest("Phone number same as your.");

            var contact = _contactRepository.CreateContact(contactDto);

            // Check for changes in the context
            var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                .ToList();

            if (!changes.Any())
            {
                return BadRequest("No changes detected.");
            }

            await _unitOfWork.CompleteAsync();

            return Ok(contact.Result);
        }

        [HttpGet("contacts-list")]
        public async Task<IEnumerable<GetContactsDto>> GetMyContacts()
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            return await _contactRepository.GetMyContacts(contactCreator);
        }

        [HttpGet("find-contact")]
        public async Task<IEnumerable<GetContactsDto>> GetContact(string name)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            return await _contactRepository.GetContactByName(name, contactCreator);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContact(int id, EditContactDto updatedContact)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            var existingContact = await _contactRepository.GetContactByIdAsync(id);
            if (existingContact == null) return NotFound();

            if (await _contactRepository.UniqueContactPhoneExists(id, updatedContact.Phone, contactCreator))
                return BadRequest("A contact with such a phone already exists in your possession");
            if (await _contactRepository.UniqueContactEmailExists(id, updatedContact.Email, contactCreator))
                return BadRequest("A contact with such an email already exists in your possession");

            _mapper.Map(updatedContact, existingContact);

            try
            {
                await _contactRepository.UpdateContactAsync(existingContact);

                var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                    .ToList();

                if (!changes.Any())
                {
                    return BadRequest("No changes detected.");
                }
                await _unitOfWork.CompleteAsync();

                return Ok("Object was edited.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the contact.");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchContact(int id, [FromBody] JsonPatchDocument<EditContactDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Invalid patch document.");
            }

            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            var existingContact = await _contactRepository.GetContactByIdAsync(id);
            if (existingContact == null) return NotFound();

            var contactToPatch = _mapper.Map<EditContactDto>(existingContact);
            patchDoc.ApplyTo(contactToPatch, ModelState);

            if (!TryValidateModel(contactToPatch))
            {
                return ValidationProblem(ModelState);
            }

            if (await _contactRepository.UniqueContactPhoneExists(id, contactToPatch.Phone, contactCreator))
                return BadRequest("A contact with such a phone already exists in your possession.");
            if (await _contactRepository.UniqueContactEmailExists(id, contactToPatch.Email, contactCreator))
                return BadRequest("A contact with such an email already exists in your possession.");

            _mapper.Map(contactToPatch, existingContact);

            try
            {
                await _contactRepository.UpdateContactAsync(existingContact);
                var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                    .ToList();

                if (!changes.Any())
                {
                    return BadRequest("No changes detected.");
                }
                await _unitOfWork.CompleteAsync();

                return Ok(existingContact);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the contact.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteContact(int id)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            if (await _contactRepository.DeleteContactAsync(id, contactCreator) == false) return BadRequest();

            var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                    .ToList();

            if (!changes.Any())
            {
                return BadRequest("No changes detected.");
            }
            await _unitOfWork.CompleteAsync();

            return Ok("Contact deleted.");
        }

        private bool isEmailValid(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
    }
}
