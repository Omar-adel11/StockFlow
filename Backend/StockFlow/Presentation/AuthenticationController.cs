using System.Security.Claims;
using Application.DTOs.AuthDTOs;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("sliding-by-ip")]

    public class AuthenticationController(
        IServiceManager serviceManager,
        SignInManager<User> signInManager,
        UserManager<User> _userManager,
        IRefreshTokenService _refreshTokenService
        ) : ControllerBase
    {

        private static readonly DateTime RefreshTokenLifetime = DateTime.UtcNow.AddDays(7);

        [HttpPost("login")]
        public async Task<IActionResult> login(LoginDTO loginDTO)
        {
            var result = await serviceManager.AuthService.LoginAsync(loginDTO);
            SetRefreshTokenCookie(result.refreshToken, RefreshTokenLifetime);
            result.refreshToken = null; // Clear the refresh token from the response body
            return Ok(result);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> signUp([FromForm] SignupDTO signupDTO)
        {
            var result = await serviceManager.AuthService.Signup(signupDTO);
            SetRefreshTokenCookie(result.refreshToken, DateTime.UtcNow.AddDays(14));
            result.refreshToken = null;
            return Ok(result);
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> changePassword(ChangePasswordDTO changePasswordDTO)
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var result = await serviceManager.AuthService.ChangePasswordAsync(changePasswordDTO, email);
            return Ok(new { message = result });
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> forgetPassword([FromBody] string email)
        {
            var result = await serviceManager.AuthService.ForgotPasswordAsync(email);
            return Ok(result);

        }

        [HttpPost("check-otp")]
        public async Task<IActionResult> CheckOtp([FromBody] CheckOtpDTO checkOtpDTO)
        {
            var result = await serviceManager.AuthService.CheckOtpAsync(checkOtpDTO);
            return Ok(result);

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> resetPassword(ResetPassDto resetPasswordDTO)
        {
            var result = await serviceManager.AuthService.ResetPasswordAsync(resetPasswordDTO);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }

            var result = await serviceManager.AuthService.refresh(refreshToken);
            SetRefreshTokenCookie(result.refreshToken, RefreshTokenLifetime);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await serviceManager.AuthService.logout(refreshToken);
                Response.Cookies.Delete("refreshToken");
            }

            Response.Cookies.Delete("refreshToken");
           
            await serviceManager.AuthService.logout(refreshToken);
            return NoContent();
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                  // Prevents JS from reading the cookie
                Secure = true,                    // Transmitted only over HTTPS ==> true (false for testing)
                SameSite = SameSiteMode.None,   // Guards against CSRF attacks 
                Expires = expiresAtUtc            // Matches refresh token expiration
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
