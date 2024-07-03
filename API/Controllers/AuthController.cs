using API.Dto;
using API.Entity;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _token;
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _cache;

        public AuthController(UserManager<AppUser> userManager, IMapper mapper, ITokenService token, IUnitOfWork unitOfWork, IUserRepository userRepository, IRedisService cache, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _mapper = mapper;
            _token = token;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
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

        [HttpPost("validate-jwt")]
        public async Task<ActionResult<UserDto>> ValidateJWT()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (jwtToken != null)
            {
                string userName = GetUserNameFromToken(jwtToken);

                var cacheToken = await _cache.GetTokenAsync($"jwt-{userName}");

                if (cacheToken != null)
                {
                    var user = await _userManager.Users.SingleOrDefaultAsync(n => n.UserName == userName && n.IsActive);

                    return new UserDto 
                    {
                        UserName = user.UserName,
                        City = user.City,
                        Country = user.Country,
                        UserEmail = user.Email,
                        Token = jwtToken
                    };
                }
                else
                {
                    return BadRequest();
                }
            }else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if(token != null)
            {
                string name = GetUserNameFromToken(token);

                string cacheKey = $"jwt-{name}";
                await _cache.RemoveTokenAsync(cacheKey);
                return Ok("Bye :>");
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower() && x.IsActive);
        }

        private async Task<bool> EmailExist(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email && x.IsActive);
        }
        private string GetUserNameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var username = jwtToken?.Claims.First(claim => claim.Type == "unique_name").Value;
            return username;
        }
    }
}