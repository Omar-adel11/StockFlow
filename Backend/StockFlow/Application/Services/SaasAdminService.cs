using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.businessOwner;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Domain.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.businessOwner.BusinessOwnerDto;

namespace Application.Services
{
    public class SaaSAdminService(IAppDbContext _context, UserManager<User> _userManager) : ISaaSAdminService
    {


        public async Task<IEnumerable<BusinessOwnerResponseDto>> GetAllBusinessOwnersAsync()
        {
            // 1. Fetch users belonging to the BusinessOwner role via Identity
            var usersInRole = await _userManager.GetUsersInRoleAsync(Roles.BusinessOwner);

            // 2. Extract IDs to filter the queryable database context
            var ownerIds = usersInRole.Select(u => u.Id).ToList();

            // 3. Project into DTOs while ignoring global query filters on tenants/businesses
            return await _context.Users
                .IgnoreQueryFilters()
                .Where(u => ownerIds.Contains(u.Id))
                .Select(u => new BusinessOwnerResponseDto
                {
                    Id = u.Id,
                    FullName = u.Name,
                    Email = u.Email,
                    IsActive = u.Business != null && u.Business.IsActive,
                    BusinessId = u.Business != null ? u.Business.Id : null,
                    BusinessName = u.Business != null ? u.Business.Name : null,
                    CurrentPlanId = u.Business != null ? u.Business.PlanId : null,
                    CurrentPlanName = u.Business != null && u.Business.Plan != null
                        ? u.Business.Plan.Name
                        : null
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateBusinessOwnerStatusAsync(int ownerId, UpdateOwnerStatusRequest request)
        {
            // Retrieve user and their business, ignoring global query filters to include deactivated businesses
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Business)
                .FirstOrDefaultAsync(u => u.Id == ownerId);

            if (user == null)
            {
                throw new UserNotFoundException();
            }
            if (user.Business == null)
            {
                throw new BusinessNotFoundException();
            }

            var business = user.Business;

            if (request.IsActive)
            {
                // 1. If activating, a valid plan is mandatory
                if (!request.PlanId.HasValue)
                {
                    throw new ArgumentException("A subscription plan must be assigned when activating a business owner.");
                }

                var planExists = await _context.Plans
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.Id == request.PlanId.Value);

                if (!planExists)
                {
                    throw new PlanNotFoundException();
                }

                business.PlanId = request.PlanId.Value;
                business.IsActive = true;
            }
            else
            {
                // Deactivate the business tenant and clear the plan assignment
                business.IsActive = false;
                business.PlanId = request.PlanId; // Assigns null (or a new plan if explicitly provided)
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
