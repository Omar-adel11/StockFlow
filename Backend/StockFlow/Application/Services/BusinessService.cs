using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IAppDbContext _dbContext;

        public BusinessService(
            IBusinessRepository businessRepository,
            IPlanRepository planRepository,
            IAppDbContext dbContext)
        {
            _businessRepository = businessRepository;
            _planRepository = planRepository;
            _dbContext = dbContext;
        }

        public async Task<bool> AssignPlanAsync(int businessId, int planId)
        {
            // 1. Validate Business Exists
            var business = await _dbContext.Business.FindAsync(businessId);
            if (business == null)
            {
                throw new BusinessNotFoundException();
            }

            // 2. Validate Plan Exists & Is Active
            var plan = await _dbContext.Plans.FindAsync(planId);
            if (plan == null)
            {
                throw new PlanNotFoundException();
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException("Cannot assign an inactive plan to a business.");
            }

            // 3. Deactivate any existing active subscriptions for this business
            var activeSubscriptions = await _dbContext.TenantSubscriptions
                .Where(s => s.BusinessId == businessId && s.Status == SubscriptionStatus.Active)
                .ToListAsync();

            foreach (var sub in activeSubscriptions)
            {
                sub.Status = SubscriptionStatus.Cancelled;
                sub.EndDateUtc = DateTime.UtcNow;
            }

            // 4. Create a new active TenantSubscription manually
            var newSubscription = new TenantSubscription
            {
                BusinessId = businessId,
                PlanId = plan.Id,
                Status = SubscriptionStatus.Active,
                StartDateUtc = DateTime.UtcNow,
                EndDateUtc = DateTime.UtcNow.AddDays(30), // Non-expiring manual grant
                AutoRenew = false,
                ExternalSubscriptionId = "MANUAL_ADMIN_ASSIGNMENT"
            };

            await _dbContext.TenantSubscriptions.AddAsync(newSubscription);

            // 5. Sync Business.PlanId reference
            business.PlanId = plan.Id;

            // 6. Save changes atomically
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
