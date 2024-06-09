using API.Data;
using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public UserRepository(DataContext dataContext, IMapper mapper, UserManager<AppUser> userManager)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<GetUsersDto>> GetUsersAsync()
        {
            var result = await _dataContext.Users.Where(x => x.IsActive == true)
                                                 .Select(x => new GetUsersDto
                                                 {
                                                     UserName = x.UserName,
                                                     City = x.City,
                                                     Country = x.Country,
                                                     Phone = x.PhoneNumber
                                                 })
                                                 .OrderBy(x => x.UserName)
                                                 .ToListAsync();

            return _mapper.Map<IEnumerable<GetUsersDto>>(result);
        }

        public async Task<bool> UniqueContactExists(string username, string phone)
        {
            return await _dataContext.Contact.AnyAsync(x => x.Name == username && x.Phone == phone);
        }

        public async Task<bool> DeleteProfile(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id.ToString() == id);

            if (user == null) return false;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
