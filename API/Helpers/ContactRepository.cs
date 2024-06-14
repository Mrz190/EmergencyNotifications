using API.Controllers;
using API.Data;
using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ContactRepository : IContactRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public ContactRepository(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<Contact> GetContactByIdAsync(int id)
        {
            return await _dataContext.Contact.FindAsync(id);
        }

        public async Task<bool> UniqueContactPhoneExists(string username, string phone, string contactCreator)
        {
            return await _dataContext.Contact.AsNoTracking().AnyAsync(x => x.Name == username && x.Phone == phone && x.CreatedBy == contactCreator);
        }

        public async Task<bool> UniqueContactPhoneExists(int id, string phone, string contactCreator)
        {
            return await _dataContext.Contact.AsNoTracking().AnyAsync(x => x.ContactId != id && x.Phone == phone && x.CreatedBy == contactCreator);
        }

        public async Task<bool> UniqueContactEmailExists(string username, string email, string contactCreator)
        {
            return await _dataContext.Contact.AsNoTracking().AnyAsync(x => x.Name == username && x.Email == email && x.CreatedBy == contactCreator);
        }

        public async Task<bool> UniqueContactEmailExists(int id, string email, string contactCreator)
        {
            return await _dataContext.Contact.AsNoTracking().AnyAsync(x => x.ContactId != id && x.Email == email && x.CreatedBy == contactCreator);
        }

        public Task<Contact> CreateContact(NewContactDto contactDto)
        {
            var contact = _mapper.Map<Contact>(contactDto);
            _dataContext.Contact.Add(contact);
            return Task.FromResult(contact);
        }

        public async Task<IEnumerable<GetContactsDto>> GetMyContacts(string contactCreator)
        {
            var result = await _dataContext.Contact.AsNoTracking().Where(x => x.CreatedBy == contactCreator)
                                                   .Select(x => new GetContactsDto
                                                   {
                                                       Id = x.ContactId,
                                                       Name = x.Name,
                                                       Phone = x.Phone,
                                                       Email = x.Email,
                                                       CreatedBy = x.CreatedBy,
                                                       CreatedAt = x.CreatedAt
                                                   })
                                                   .OrderBy(x => x.Name)
                                                   .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<GetContactsDto>> GetContactByName(string name, string contactCreator)
        {
            var result = await _dataContext.Contact.Where(x => x.Name.Contains(name) && x.CreatedBy == contactCreator)
                                                   .Select(x => new GetContactsDto
                                                   {
                                                       Id = x.ContactId,
                                                       Name = x.Name,
                                                       Phone = x.Phone,
                                                       Email = x.Email,
                                                       CreatedBy = x.CreatedBy,
                                                       CreatedAt = x.CreatedAt
                                                   })
                                                   .OrderBy(x => x.Name).AsNoTracking()
                                                   .ToListAsync();

            return result;
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            _dataContext.Entry(contact).State = EntityState.Modified;
        }

        public async Task<IEnumerable<Recipient>> GetContactsForMail(List<Recipient> recipients)
        {
            var ids = recipients.Select(r => r.Id).ToList();
            var mails = recipients.Select(r => r.Mail).ToList();

            return await _dataContext.Contact
                                     .Where(c => ids.Contains(c.ContactId) && mails.Contains(c.Email))
                                     .Select(c => new Recipient
                                     {
                                         Id = c.ContactId,
                                         Mail = c.Email
                                     })
                                     .ToListAsync();
        }

        public async Task<bool> DeleteContactAsync(int id, string contactCreator)
        {
            var itemToRemove = _dataContext.Contact.SingleOrDefault(x => x.ContactId == id && x.CreatedBy == contactCreator);

            if (itemToRemove != null)
            {
                _dataContext.Contact.Remove(itemToRemove);
                return true;
            }
            return false;
        }
    }
}
