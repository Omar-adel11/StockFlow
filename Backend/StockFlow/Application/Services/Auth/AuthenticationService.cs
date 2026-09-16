using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.DTOs.userDtos;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Application.Services.Helper;
using Domain.Entities;
using Domain.Exceptions.BadRequest;
using Domain.Exceptions.NotFound;
using Domain.Exceptions.Unauthorized;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;


namespace Application.Services.Auth
{
    public class AuthenticationService(
        UserManager<User> _userManager,
        ITokenService _tokenService,
        IRefreshTokenService _refreshTokenService,
        IOTPService _oTPService,
        IEmailService _emailService,
        IHostingEnvironment _env) : IAuthenticationService
    {
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        public async Task<UserDTO?> Login(LoginDTO loginDTO)
        {
            var user = await CheckEmailExistence(loginDTO.email);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.password);
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }

            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = await _refreshTokenService.GenerateAndStoreAsync(user.Id, RefreshTokenLifetime);


            return new UserDTO
            {
                email = user.Email,
                name = user.UserName,
                Token = accessToken,
                ImgUrl = user.ImgUrl,
                refreshToken = refreshToken
            };
        }
        public async Task<UserDTO?> refresh(RefreshRequestDto refreshRequestDto)
        {
            var userId = await _refreshTokenService.ValidateAndGetUserIdAsync(refreshRequestDto.RefreshToken);
            if (userId is null)
            {
                throw new InvalidCredentialsException();
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            // Rotation: the old token is revoked and a brand new one issued.

            await _refreshTokenService.RevokeAsync(refreshRequestDto.RefreshToken);
            var newRefreshToken = await _refreshTokenService.GenerateAndStoreAsync(user.Id, RefreshTokenLifetime);
            var newAccessToken = await _tokenService.GenerateToken(user);

            return new UserDTO
            {
                email = user.Email,
                name = user.UserName,
                Token = newAccessToken,
                ImgUrl = user.ImgUrl,
                refreshToken = newRefreshToken
            };
        }

        public async Task<UserDTO?> Signup(SignupDTO signupDTO)
        {
            var user = new User
            {
                Email = signupDTO.email,
                UserName = signupDTO.username,
            };
            var EmailExistence = await _userManager.FindByEmailAsync(user.Email);
            if(EmailExistence is not null)
            {
                throw new EmailExistsException();
            }

            var result = await _userManager.CreateAsync(user, signupDTO.password);
            if (!result.Succeeded)
            {
                throw new RegisterationBadRequestException(result.Errors.Select(e => e.Description));
            }
            //add photo if provided
            if (signupDTO.file != null)
            {
                user.ImgUrl = DocumentSettings.UploadFile(signupDTO.file, _env.WebRootPath, "images");
                await _userManager.UpdateAsync(user);
            }
            return new UserDTO
            {
                email = user.Email,
                name = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                ImgUrl = user.ImgUrl
            };
        }

        public async Task<string> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO, string email)
        {
            var user = await CheckEmailExistence(email);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordDTO.password);
            if (!isPasswordValid)
            {
                throw new InvalidOldPasswordException();
            }
            await _userManager.ChangePasswordAsync(user, changePasswordDTO.password, changePasswordDTO.newPassword);
            return "Password changed successfully";

        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await CheckEmailExistence(email);
            var otp = await _oTPService.GenerateOTP(email);
            
            await _emailService.SendEmailAsync(email, "Password Reset OTP", $"Your OTP is: {otp}");
            return "OTP sent to email";
        }

        public async Task<string> CheckOtpAsync(CheckOtpDTO checkOtpDTO)
        {
            var user = await CheckEmailExistence(checkOtpDTO.email);
            OTPDTO oTPDTO = new OTPDTO
            {
                email = checkOtpDTO.email,
                otp = checkOtpDTO.otp
            };
            var ResetToken = await _oTPService.VerifyOTP(oTPDTO);
            return ResetToken.ToString();

        }

        public async Task<string> ResetPasswordAsync(ResetPassDto resetPassDto)
        {
            var user = await CheckEmailExistence(resetPassDto.email);
            var result = await _userManager.ResetPasswordAsync(user, resetPassDto.ResetToken, resetPassDto.Password);

            if (!result.Succeeded)
            {
                throw new ResetPasswordBadRequestException(result.Errors.Select(e => e.Description));
            }
            return "Password reset successfully";
        }

        public async Task logout(RefreshRequestDto refreshRequestDto)
        {
            await _refreshTokenService.RevokeAsync(refreshRequestDto.RefreshToken);
        }

        private async Task<User?> CheckEmailExistence(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            return user;
        }




    }
}
