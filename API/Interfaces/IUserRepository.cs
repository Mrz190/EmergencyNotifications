using API.Dto;
using API.Entity;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<GetUsersDto>> GetUsersAsync();
        Task<bool> DeleteProfile(string id);
    }
}
