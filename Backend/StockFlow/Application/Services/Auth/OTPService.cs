using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.Interfaces.AuthInterfaces;
using Domain.Entities;
using Domain.Exceptions.Unauthorized;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services.Auth
{
    public class OTPService(UserManager<User> _userManager, ILogger<OTPService> _logger) : IOTPService
    {
        public async Task<string> GenerateOTP(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            _logger.LogInformation("Password reset OTP generated. UserId: {UserId}", user.Id);
            return otp;
        }

        public async Task<string> VerifyOTP(OTPDTO oTPDTO)
        {
            var user = await _userManager.FindByEmailAsync(oTPDTO.email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, oTPDTO.otp);
            if (isValid)
            {
                _logger.LogInformation("Password reset OTP verified successfully. UserId: {UserId}", user.Id);
                return await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            else
            {
                _logger.LogWarning("Password reset OTP verification failed. UserId: {UserId}", user.Id);
                throw new InvalidOtpException();
            }

        }
    }
}
