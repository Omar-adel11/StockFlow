using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Services
{
    public class SaaSAdminService(IAppDbContext _context, UserManager<User> _userManager,ISubscriptionService subscriptionService) : ISaaSAdminService
    {


        public async Task<IEnumerable<BusinessOwnerResponseDto>> GetAllBusinessOwnersAsync(string? search)
        {
            // 1. Fetch users belonging to the BusinessOwner role via Identity
            var usersInRole = await _userManager.GetUsersInRoleAsync(Roles.BusinessOwner);

            // 2. Extract IDs to filter the queryable database context
            var ownerIds = usersInRole.Select(u => u.Id).ToList();

            // 3. Project into DTOs while ignoring global query filters on tenants/businesses
            var query = _context.Users
                .IgnoreQueryFilters()
                .Where(u => ownerIds.Contains(u.Id));

            if(!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                u.Name.Contains(search) || u.Email.Contains(search) ||(u.Business != null && u.Business.Name.Contains(search)));
            }


                return await query.Select(u => new BusinessOwnerResponseDto
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
                if (!request.PlanId.HasValue)
                    throw new ArgumentException("A subscription plan must be assigned when activating a business owner.");

                var plan = await _context.Plans.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == request.PlanId.Value)
                    ?? throw new PlanNotFoundException();

                business.PlanId = plan.Id;
                business.IsActive = true;
                return await subscriptionService.ActivateSubscription(
                    business.Id, plan.Id, plan.BillingCycle == 0 ? 30 : 365);
            }

            // Deactivation must work even if the UI sends a null planId.
            var activeSubscription = await _context.TenantSubscriptions.IgnoreQueryFilters()
                .Where(s => s.BusinessId == business.Id
                    && s.Status == Domain.Entities.Enum.SubscriptionStatus.Active
                    && s.EndDateUtc > DateTime.UtcNow)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (activeSubscription != null)
                await subscriptionService.DeactivateSubscription(business.Id, activeSubscription.PlanId);

            business.IsActive = false;
            business.PlanId = null;
            await _context.SaveChangesAsync();
            return true;

           
        }
    }
}
