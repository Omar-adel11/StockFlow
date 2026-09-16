using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.Interfaces.AuthInterfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Auth
{
    public class OTPService(UserManager<User> _userManager) : IOTPService
    {
        public async Task<string> GenerateOTP(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var otp = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
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
                return await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            else
            {
                return "Invalid OTP";
            }

        }
    }
}
