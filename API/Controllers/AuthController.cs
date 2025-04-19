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
    [ApiController]
    [Route("api/[controller]")]
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var existingUser = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        (x.UserName == regDto.UserName.ToLower() || x.Email == regDto.Email) && x.IsActive);

                if (existingUser != null)
                {
                    if (existingUser.UserName == regDto.UserName.ToLower())
                        return BadRequest("User already exists.");
                    return BadRequest("Account with this email already exists.");
                }

                var user = _mapper.Map<AppUser>(regDto);
                user.UserName = regDto.UserName.ToLower();
                user.IsActive = true;

                var result = await _userManager.CreateAsync(user, regDto.Password);
                if (!result.Succeeded) return BadRequest("Failed to create user.");

                if (!await _userManager.IsInRoleAsync(user, "Member"))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Member");
                    if (!roleResult.Succeeded) return BadRequest("Failed to assign role.");
                }

                string token = await _token.CreateToken(user);
                var cacheKey = $"jwt-{user.UserName}";
                await _cache.SetTokenAsync(cacheKey, token, TimeSpan.FromHours(2));

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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(n => n.UserName == loginDto.UserName && n.IsActive);

            if (user == null) return Unauthorized("Invalid username/password.");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result) return Unauthorized("Invalid username/password.");

            string token = await _token.CreateToken(user);
            string cacheKey = $"jwt-{user.UserName}";
            await _cache.SetTokenAsync(cacheKey, token, TimeSpan.FromHours(2));

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
            var jwtToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(jwtToken)) return Unauthorized("Missing token.");

            string userName = GetUserNameFromToken(jwtToken);
            if (string.IsNullOrEmpty(userName)) return Unauthorized("Invalid token.");

            var cacheToken = await _cache.GetTokenAsync($"jwt-{userName}");
            if (cacheToken == null) return Unauthorized("Token expired or revoked.");

            var user = await _userManager.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(n => n.UserName == userName && n.IsActive);

            if (user == null) return Unauthorized("User not found.");

            return new UserDto
            {
                UserName = user.UserName,
                City = user.City,
                Country = user.Country,
                UserEmail = user.Email,
                Token = jwtToken
            };
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token)) return BadRequest("Token missing.");

            string name = GetUserNameFromToken(token);
            if (string.IsNullOrEmpty(name)) return BadRequest("Token invalid.");

            string cacheKey = $"jwt-{name}";
            await _cache.RemoveTokenAsync(cacheKey);
            return Ok("Logged out successfully.");
        }

        private string GetUserNameFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
