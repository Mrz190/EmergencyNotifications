using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _token;
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _cache;

        public AuthController(UserManager<AppUser> userManager, IMapper mapper, ITokenService token, IUnitOfWork unitOfWork, IUserRepository userRepository, IRedisService cache)
        {
            _userManager = userManager;
            _mapper = mapper;
            _token = token;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _cache = cache;
        }

        [HttpPost("reg")]
        public async Task<ActionResult<UserDto>> CreateMember(RegDto regDto)
        {
            try
            {
                if (await UserExists(regDto.UserName)) return BadRequest("User already exists.");
                if (await EmailExist(regDto.Email)) return BadRequest("Account with this email already exists.");

                var user = _mapper.Map<AppUser>(regDto);
                user.UserName = regDto.UserName.ToLower();
                user.IsActive = true;

                var result = await _userManager.CreateAsync(user, regDto.Password);
                if (!result.Succeeded) throw new Exception("Failed to create user.");

                var roleExists = await _userManager.IsInRoleAsync(user, "Member");
                if (!roleExists)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Member");
                    if (!roleResult.Succeeded) throw new Exception("Failed to assign role.");
                }

                string token = await _token.CreateToken(user);

                var cacheKey = $"jwt-{user.UserName}";
                await _cache.SetTokenAsync(cacheKey, token);

                return new UserDto
                {
                    UserName = user.UserName,
                    City = user.City,
                    Country = user.Country,
                    UserEmail = user.Email,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(n => n.UserName == loginDto.UserName && n.IsActive);
            if (user == null) return BadRequest("Invalid username/password.");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result) return BadRequest("Invalid username/password.");

            string token = await _token.CreateToken(user);
            string cacheKey = $"jwt-{user.UserName}";
            await _cache.SetTokenAsync(cacheKey, token);

            return new UserDto
            {
                UserName = user.UserName,
                City = user.City,
                Country = user.Country,
                UserEmail = user.Email,
                Token = token
            };
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout(LogoutDto logoutDto)
        {
            string cacheKey = $"jwt-{logoutDto.UserName}";
            await _cache.RemoveTokenAsync(cacheKey);
            return Ok();
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower() && x.IsActive);
        }

        private async Task<bool> EmailExist(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email && x.IsActive);
        }
    }
}