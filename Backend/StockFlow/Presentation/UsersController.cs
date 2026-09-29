using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.userDtos;
using Application.Interfaces;
using Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

       
        [HttpGet("me")]
        
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetUserProfileAsync(userId);
            return Ok(profile);
           
        }

        
        [HttpPut("me")]
        
        public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateUserProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var updatedProfile = await _userService.UpdateUserProfileAsync(userId, dto);
            return Ok(updatedProfile);
           
        }

        #region Helper Methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
        #endregion
    }
}