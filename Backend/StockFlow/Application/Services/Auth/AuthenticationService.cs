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
using Domain.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Application.Services.Auth
{
    public class AuthenticationService(
        UserManager<User> _userManager,
        ITokenService _tokenService,
        IRefreshTokenService _refreshTokenService,
        IOTPService _oTPService,
        IEmailService _emailService,
        IHostingEnvironment _env,
        IAppDbContext _dbContext) : IAuthenticationService
    {
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        public async Task<UserDTO?> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .Include(u => u.Business)
                .FirstOrDefaultAsync(u => u.Email == loginDTO.email);

            if (user == null)
            {
                throw new InvalidCredentialsException();
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Your account has been deactivated. Please contact your business owner or administrator.");
            }

            if (user.Business != null && !user.Business.IsActive)
            {
                throw new UnauthorizedAccessException("Your business subscription has been deactivated. Please contact your administrator.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.password);
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }

            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = await _refreshTokenService.GenerateAndStoreAsync(user.Id, RefreshTokenLifetime);

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                email = user.Email,
                name = user.Name,
                Token = accessToken,
                ImgUrl = user.ImgUrl,
                refreshToken = refreshToken,
                Role = roles.FirstOrDefault(),
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
                name = user.Name,
                Token = newAccessToken,
                ImgUrl = user.ImgUrl,
                refreshToken = newRefreshToken
            };
        }

        public async Task<UserDTO?> Signup(SignupDTO signupDTO)
        {

            var EmailExistence = await _userManager.FindByEmailAsync(signupDTO.email);
            if (EmailExistence is not null)
            {
                throw new EmailExistsException();
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create Business first (PlanId defaults to null)
                var business = new Business
                {
                    Name = signupDTO.BusinessName,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbContext.Business.Add(business);
                await _dbContext.SaveChangesAsync();

                var randomNumber = Random.Shared.Next(1000, 10000);
                // Create User linked to the new Business
                var user = new User
                {
                    Email = signupDTO.email,
                    Name = signupDTO.name,
                    UserName = $"{signupDTO.name}{randomNumber}",
                    PhoneNumber = signupDTO.PhoneNumber,
                    BusinessId = business.Id,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, signupDTO.password);
                if (!result.Succeeded)
                {

                    await transaction.RollbackAsync();
                    throw new RegisterationBadRequestException(result.Errors.Select(e => e.Description));
                }
                //add photo if provided
                if (signupDTO.file != null)
                {
                    user.ImgUrl = DocumentSettings.UploadFile(signupDTO.file, _env.WebRootPath, "images");
                    await _userManager.UpdateAsync(user);
                }
               
               
                // Assign "BusinessOwner" role
                var roleResult = await _userManager.AddToRoleAsync(user, Roles.BusinessOwner);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Failed to assign BusinessOwner role.");
                }

                // Commit transaction
                await transaction.CommitAsync();

                // 3. Generate JWT Token
                var roles = await _userManager.GetRolesAsync(user);
                var token = await _tokenService.GenerateToken(user);
                var newRefreshToken = await _refreshTokenService.GenerateAndStoreAsync(user.Id, RefreshTokenLifetime);

                return new UserDTO
                {
                    Token = token,
                    email = user.Email,
                    name = user.Name,
                    Role = Roles.BusinessOwner,
                    refreshToken = newRefreshToken,
                    ImgUrl = user.ImgUrl
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
