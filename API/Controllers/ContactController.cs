﻿using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class ContactController : BaseApiController
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        public ContactController(IContactRepository contactRepository, IMapper mapper)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
        }

        [HttpPost("add-contact")]
        public async Task<ActionResult<Contact>> AddContact(NewContactDto contactDto)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            if (await _contactRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, contactCreator)) return NotFound("Contact with this phone already exists");
            if (await _contactRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, contactCreator)) return NotFound("Contact with this email already exists");

            var contactCreatorName = User.FindFirstValue(ClaimTypes.Name);
            var contactCreatorEmail = User.FindFirst(ClaimTypes.Email).ToString();
            contactDto.CreatedBy = contactCreatorName;

            if (contactCreatorEmail == contactDto.Email) return BadRequest("Contact email same as your.");

            var contact = _contactRepository.CreateContact(contactDto);

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
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Произошла ошибка при обновлении контакта");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchContact(int id, [FromBody] JsonPatchDocument<EditContactDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Invalid patch document");
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
                return BadRequest("A contact with such a phone already exists in your possession");
            if (await _contactRepository.UniqueContactEmailExists(id, contactToPatch.Email, contactCreator))
                return BadRequest("A contact with such an email already exists in your possession");

            _mapper.Map(contactToPatch, existingContact);

            try
            {
                await _contactRepository.UpdateContactAsync(existingContact);
                return Ok(existingContact);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the contact");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteContact(int id)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            if (await _contactRepository.DeleteContactAsync(id, contactCreator) == false)
                return BadRequest();
            return Ok("Contact deleted");
        }
    }
}
