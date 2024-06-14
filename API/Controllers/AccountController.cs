using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using API.Entity;

namespace API.Controllers
{
    [Authorize]
    public class AccountController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        [HttpDelete("delete-profile")]
        public async Task<ActionResult> DeleteProfile(string checkPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, checkPassword);

            if (!passwordValid)
            {
                return Unauthorized("Invalid password.");
            }

            var result = await _userRepository.DeleteProfile(userId);

            if (result)
            {
                await _unitOfWork.CompleteAsync();
                return Ok("Your profile was successfully deleted.");
            }

            return BadRequest("There was an issue deleting your profile.");
        }
    }
}
