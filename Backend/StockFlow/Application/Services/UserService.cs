using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.DTOs.userDtos;
using Application.Interfaces;
using Application.Services.Helper;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UserService(UserManager<User> userManager, IHostingEnvironment  env) : IUserService
    {
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var roles = await userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ImgUrl = user.ImgUrl,
                Role = roles.FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            // Update Name
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                user.Name = dto.Name;
            }

            // Update Phone Number
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            // Update Email & Username
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var emailOwner = await userManager.FindByEmailAsync(dto.Email);
                if (emailOwner != null && emailOwner.Id != userId)
                {
                    throw new InvalidOperationException("Email is already taken by another account.");
                }

                user.Email = dto.Email;
                user.NormalizedEmail = userManager.KeyNormalizer?.NormalizeEmail(dto.Email);
                user.UserName = dto.Email;
                user.NormalizedUserName = userManager.KeyNormalizer?.NormalizeName(dto.Email);
            }

            // Update Profile Image
            if (dto.file != null && dto.file.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ImgUrl))
                {
                    DocumentSettings.DeleteFile(user.ImgUrl, env.WebRootPath, "images");
                }

                user.ImgUrl = DocumentSettings.UploadFile(dto.file, env.WebRootPath, "images");
            }

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update profile: {errors}");
            }

            return await GetUserProfileAsync(userId);
        }
    }
}