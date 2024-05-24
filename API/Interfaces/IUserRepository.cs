using API.Dto;
using API.Entity;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<GetUsersDto>> GetUsersAsync();
        Task<bool> UniqueContactPhoneExists(string username, string phone, string contactCreator);
        Task<bool> UniqueContactEmailExists(string username, string email, string contactCreator);
        Task<Contact> CreateContact(NewContactDto contactDto);
        Task<bool> SaveChangesAsync();

        Task<IEnumerable<GetContactsDto>> GetMyContacts(string contactCreator);
        Task<IEnumerable<GetContactsDto>> GetContactByName(string name, string contactCreator);
        Task<bool> DeleteProfile(string id);
    }
}
