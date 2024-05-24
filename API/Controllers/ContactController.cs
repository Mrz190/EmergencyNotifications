using API.Dto;
using API.Entity;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class ContactController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        public ContactController(IUserRepository userRepository)
        {
             _userRepository = userRepository;
        }


        [HttpPost("add-contact")]
        public async Task<ActionResult<Contact>> AddContact(NewContactDto contactDto)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            if (await _userRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, contactCreator)) return NotFound("Contact with this phone already exists");
            if (await _userRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, contactCreator)) return NotFound("Contact with this email already exists");

            var contactCreatorName = User.FindFirstValue(ClaimTypes.Name);
            var contactCreatorEmail = User.FindFirst(ClaimTypes.Email).ToString();
            contactDto.CreatedBy = contactCreatorName;

            if (contactCreatorEmail == contactDto.Email) return BadRequest("Contact email same as your.");

            var contact = _userRepository.CreateContact(contactDto);

            return Ok(contact.Result);
        }

        [HttpGet("contacts-list")]
        public async Task<IEnumerable<GetContactsDto>> GetMyContacts()
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);

            return await _userRepository.GetMyContacts(contactCreator);
        }

        [HttpGet("find-contact")]
        public async Task<IEnumerable<GetContactsDto>> GetContact(string name)
        {
            var contactCreator = User.FindFirstValue(ClaimTypes.Name);
            return await _userRepository.GetContactByName(name, contactCreator);
        }
    }
}
