using API.Controllers;
using API.Dto;
using API.Entity;

namespace API.Interfaces
{
    public interface IContactRepository
    {
        Task<Contact> GetContactByIdAsync(int id);
        Task<bool> UniqueContactPhoneExists(string username, string phone, string contactCreator);
        Task<bool> UniqueContactPhoneExists(int id, string phone, string contactCreator);
        Task<bool> UniqueContactEmailExists(string username, string email, string contactCreator);
        Task<bool> UniqueContactEmailExists(int id, string email, string contactCreator);
        Task<Contact> CreateContact(NewContactDto contactDto);
        Task<IEnumerable<GetContactsDto>> GetMyContacts(string contactCreator);
        Task<IEnumerable<GetContactsDto>> GetContactByName(string name, string contactCreator);
        Task UpdateContactAsync(Contact contact);
        Task<IEnumerable<Recipient>> GetContactsForMail(List<Recipient> recipients);
        Task<bool> DeleteContactAsync(int id, string contactCreator);    
        Task<bool> SaveChangesAsync();
    }
}
