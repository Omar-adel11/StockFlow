using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Application.DTOs.AuthDTOs;
using Application.DTOs.Team;
using Application.DTOs.userDtos;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Application.Services.Auth;
using Application.Services.Helper;
using Domain.Entities;
using Domain.Exceptions.BadRequest;
using Domain.Exceptions.NotFound;
using Domain.Helpers;
using Domain.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TeamService : ITeamService
    {

        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IHostingEnvironment _env;
        private readonly IAppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService; // Assuming you have an IEmailService
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public TeamService(
            IAppDbContext dbContext,
            UserManager<User> userManager,
            ITokenService tokenService,
            IEmailService emailService,
            IRefreshTokenService refreshTokenService,
            IHostingEnvironment env,
            RoleManager<IdentityRole<int>> roleManager
            )
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _refreshTokenService = refreshTokenService;
            _env = env;
            _roleManager = roleManager; 
        }
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        public async Task SendInviteAsync(SendInviteRequest request, int currentUserId, int currentBusinessId)
        {
            // 1. Enforce Role Restriction: Can only invite "Manager" or "Staff"
            if (request.Role != Roles.Manager && request.Role != Roles.Staff)
            {
                throw new InvalidOperationException("You can only send invitations for 'Manager' or 'Staff' roles.");
            }

            // 2. Check if email is already registered as a active user in the system
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email address already exists.");
            }

            // 3. Generate a secure unguessable random token
            var tokenBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToHexString(tokenBytes);

            // 4. Save Invitation to DB
            var invitation = new TeamInvitation
            {
                Email = request.Email,
                Role = request.Role,
                BusinessId = currentBusinessId,
                InvitedByUserId = currentUserId,
                Token = token,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.TeamInvitations.Add(invitation);
            await _dbContext.SaveChangesAsync();

            // 5. Send Email with invite URL
            var business = await _dbContext.Business.FindAsync(currentBusinessId);
            var inviteLink = $"http://localhost:5500/accept-invite?token={token}";

            await _emailService.SendEmailAsync(
                request.Email,
                $"You've been invited to join {business?.Name ?? "our team"}",
                $"<p>You have been invited to join <strong>{business?.Name}</strong> as a <strong>{request.Role}</strong>.</p>" +
                $"<p>Click <a href='{inviteLink}'>here</a> to accept your invitation and set up your account.</p>"
            );
        }

        public async Task<InviteDetailsResponse> GetInviteDetailsAsync(string token)
        {
            var invite = await _dbContext.TeamInvitations
                .Include(ti => ti.Business).IgnoreQueryFilters()
                .FirstOrDefaultAsync(ti => ti.Token == token);

            if (invite == null)
            {
                return new InviteDetailsResponse { IsValid = false, ErrorMessage = "Invalid invitation token." };
            }

            if (!invite.IsActive)
            {
                string reason = invite.AcceptedAtUtc.HasValue ? "Invitation has already been used."
                              : invite.RevokedAtUtc.HasValue ? "Invitation has been revoked."
                              : "Invitation has expired.";
                return new InviteDetailsResponse { IsValid = false, ErrorMessage = reason };
            }

            return new InviteDetailsResponse
            {
                IsValid = true,
                Email = invite.Email,
                BusinessName = invite.Business.Name,
                Role = invite.Role
            };
        }

        public async Task<UserDTO> AcceptInviteAsync(AcceptInviteRequest acceptInviteDTO)
        {
        // 1. Fetch & Validate Invitation
        var invite = await _dbContext.TeamInvitations
            .FirstOrDefaultAsync(ti => ti.Token == acceptInviteDTO.Token);

        if (invite is null || !invite.IsActive)
        {
            throw new InvalidOperationException("This invitation link is invalid or expired.");
        }

        // 2. Double check email registration status
        var existingUser = await _userManager.FindByEmailAsync(invite.Email);
        if (existingUser is not null)
        {
            throw new EmailExistsException();
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var randomNumber = Random.Shared.Next(1000, 10000);

            // 3. Create User bound to invitation's BusinessId & Email
            var user = new User
            {
                Email = invite.Email,
                Name = acceptInviteDTO.Name,
                UserName = $"{acceptInviteDTO.Name}{randomNumber}",
                PhoneNumber = acceptInviteDTO.PhoneNumber,
                BusinessId = invite.BusinessId, // Strictly scoped to the inviting business
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, acceptInviteDTO.Password);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                throw new RegisterationBadRequestException(result.Errors.Select(e => e.Description));
            }

            // 4. Add profile photo if provided
            if (acceptInviteDTO.file != null)
            {
                user.ImgUrl = DocumentSettings.UploadFile(acceptInviteDTO.file, _env.WebRootPath, "images");
                await _userManager.UpdateAsync(user);
            }

            // 5. Assign role from the invitation record ("Manager" or "Staff")
            var roleResult = await _userManager.AddToRoleAsync(user, invite.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to assign {invite.Role} role.");
            }

            // 6. Mark invitation as accepted
            invite.AcceptedAtUtc = DateTime.UtcNow;
            _dbContext.TeamInvitations.Update(invite);
            await _dbContext.SaveChangesAsync();

            // 7. Commit transaction
            await transaction.CommitAsync();

            // 8. Generate JWT & Refresh Tokens
            var token = await _tokenService.GenerateToken(user);
                    var newRefreshToken = await _refreshTokenService.GenerateAndStoreAsync(user.Id, RefreshTokenLifetime);

            return new UserDTO
            {
                Token = token,
                email = user.Email,
                name = user.Name,
                Role = invite.Role,
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

        public async Task CancelInviteAsync(int inviteId, int currentBusinessId)
        {
            var invite = await _dbContext.TeamInvitations
                .FirstOrDefaultAsync(ti => ti.Id == inviteId && ti.BusinessId == currentBusinessId);

            if (invite != null && invite.IsActive)
            {
                invite.RevokedAtUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<TeamInvitationDto>> GetPendingInvitesAsync(int currentBusinessId)
        {
            return await _dbContext.TeamInvitations
                .Where(ti => ti.BusinessId == currentBusinessId && ti.AcceptedAtUtc == null && ti.RevokedAtUtc == null && ti.ExpiresAtUtc > DateTime.UtcNow)
                .Select(ti => new TeamInvitationDto
                {
                    Id = ti.Id,
                    Email = ti.Email,
                    Role = ti.Role,
                    CreatedAtUtc = ti.CreatedAtUtc,
                    ExpiresAtUtc = ti.ExpiresAtUtc
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int businessId)
        {
            // Join Users with UserRoles and Roles to fetch team members scoped to the tenant
            var members = await (from user in _dbContext.Users
                                 join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                                 join role in _dbContext.Roles on userRole.RoleId equals role.Id
                                 where user.BusinessId == businessId
                                 select new TeamMemberDto
                                 {
                                     Id = user.Id,
                                     Name = user.Name,
                                     Email = user.Email!,
                                     Role = role.Name!,
                                     BusinessId = user.BusinessId ?? 0
                                 }).ToListAsync();

            return members;
        }

        public async Task<TeamMemberDto?> GetMemberByIdAsync(int memberId, int businessId)
        {
            var member = await (from user in _dbContext.Users
                                join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                                join role in _dbContext.Roles on userRole.RoleId equals role.Id
                                where user.Id == memberId && user.BusinessId == businessId
                                select new TeamMemberDto
                                {
                                    Id = user.Id,
                                    Name = user.Name,
                                    Email = user.Email!,
                                    Role = role.Name!,
                                    BusinessId = user.BusinessId ?? 0
                                }).FirstOrDefaultAsync();

            return member;
        }

        public async Task RemoveMemberAsync(int memberId, int businessId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == memberId && u.BusinessId == businessId);

            if (user == null)
            {
                throw new KeyNotFoundException("Team member not found in this business.");
            }

            // Check if member is a BusinessOwner by querying UserRoles and Roles
            var isOwner = await (from ur in _dbContext.UserRoles
                                 join r in _dbContext.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == memberId && r.Name == Roles.BusinessOwner
                                 select r).AnyAsync();

            if (isOwner)
            {
                var ownerCount = await (from u in _dbContext.Users
                                        join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dbContext.Roles on ur.RoleId equals r.Id
                                        where u.BusinessId == businessId && r.Name == Roles.BusinessOwner
                                        select u).CountAsync();

                if (ownerCount <= 1)
                {
                    throw new InvalidOperationException("Cannot remove the primary Business Owner.");
                }
            }

            // Soft-delete user & decouple from business tenant
            user.IsActive = false;
            user.BusinessId = null;

            // Revoke all active refresh tokens so they cannot get new access tokens via /refresh
            /* 
             
             */
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateMemberRoleAsync(int memberId, string newRole, int businessId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == memberId && u.BusinessId == businessId);

            if (user == null)
            {
                throw new UserNotFoundException();
            }

            // Check existing owner role using UserRoles & Roles tables
            var isCurrentOwner = await (from ur in _dbContext.UserRoles
                                        join r in _dbContext.Roles on ur.RoleId equals r.Id
                                        where ur.UserId == memberId && r.Name == Roles.BusinessOwner
                                        select r).AnyAsync();

            if (isCurrentOwner && newRole != Roles.BusinessOwner)
            {
                var ownerCount = await (from u in _dbContext.Users
                                        join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dbContext.Roles on ur.RoleId equals r.Id
                                        where u.BusinessId == businessId && r.Name == Roles.BusinessOwner
                                        select u).CountAsync();

                if (ownerCount <= 1)
                {
                    throw new InvalidOperationException("Cannot change role. Business must have at least one Business Owner.");
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);
        }
    }
}