using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.Payment.Paymentdtos;
using static Application.DTOs.Subscriptions.Subscriptiondtos;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IAppDbContext _dbContext;

        public SubscriptionService( IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        

        public async Task<SubscriptionDto> StartFreeTrialAsync(
            int businessId,
            int planId,
            CancellationToken ct = default)
        {
            var business = await _dbContext.Business.IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == businessId, ct)
                ?? throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
            bool hasActiveSubscription = await _dbContext.TenantSubscriptions
                .AnyAsync(s => s.BusinessId == businessId
                            && s.Status == SubscriptionStatus.Active
                            && s.EndDateUtc > DateTime.UtcNow, ct);

            if (hasActiveSubscription)
            {
                throw new InvalidOperationException("Business already has an active subscription and cannot start a free trial.");
            }
            var plan = await _dbContext.Plans
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsFreeTrial, ct)
                ?? throw new InvalidOperationException("The requested plan is not a valid Free Trial plan.");

            // Domain validation check
            if (!business.CanStartFreeTrial())
            {
                throw new InvalidOperationException("This account is not eligible for a free trial.");
            }

            DateTime now = DateTime.UtcNow;
            // Mark free trial as used permanently
            business.MarkFreeTrialUsed();
            var result = await ActivateSubscription(businessId, planId, 14);

            if(result)
            {
                var subscription = await _dbContext.TenantSubscriptions
                .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.Status == SubscriptionStatus.Active)
                ?? throw new SubscriptionNotFoundException();


                return new SubscriptionDto
                {
                    Id = subscription.Id,                  // Primary key from TenantSubscriptions table
                    BusinessId = subscription.BusinessId,
                    PlanId = plan.Id,
                    PlanName = plan.Name,                  // Name from Plan entity
                    Status = subscription.Status.ToString(),
                    StartDateUtc = subscription.StartDateUtc,
                    EndDateUtc = subscription.EndDateUtc,
                    IsActive = subscription.EndDateUtc > DateTime.UtcNow
                };
            }
            business.UndoMarkFreeTrialUsed();
            throw new InvalidOperationException("can't create subscription");
        }

        public async Task<TenantSubscriptionResponse?> GetCurrentSubscriptionAsync(
           int businessId,
           CancellationToken ct = default)
        {
            DateTime now = DateTime.UtcNow;

            var subs =  await _dbContext.TenantSubscriptions
                .Where(s => s.BusinessId == businessId
                 && s.Status == SubscriptionStatus.Active
                 && s.EndDateUtc > now)
        .OrderByDescending(s => s.EndDateUtc)
                .Select(s => new TenantSubscriptionResponse(
                    s.Id,
                    s.BusinessId,
                    s.PlanId,
                    s.Plan.Name,
                    s.Status,
                    s.StartDateUtc,
                    s.EndDateUtc,
                    s.AutoRenew))
                .FirstOrDefaultAsync(ct);
            if (subs == null)
            {
                throw new SubscriptionNotFoundException();
            }
            return subs;
        }

        //add tenant subscription
        public async Task<bool> ActivateSubscription(int businessId,int planId,int days)
        {
            var business = await _dbContext.Business
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
                throw new KeyNotFoundException($"Business with ID {businessId} was not found.");

            var plan = await _dbContext.Plans
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
            {
                throw new PlanNotFoundException();
            }


            DateTime now = DateTime.UtcNow;


            var subscription = await _dbContext.TenantSubscriptions
                .IgnoreQueryFilters()
                .Where(s => s.BusinessId == businessId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDateUtc > now)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            foreach (var activeSub in subscription)
            {
                if (activeSub.Plan != null && activeSub.Plan.IsFreeTrial)
                {
                    // Cancel active free trial to make way for paid plan
                    activeSub.Status = SubscriptionStatus.Cancelled;
                    activeSub.EndDateUtc = now;
                }
                else
                {
                    // Active paid plan protection
                    throw new InvalidOperationException("Business already has an active paid subscription. Cancel it before subscribing to a new plan.");
                }
            }

            var newSubscription = new TenantSubscription
            {
                BusinessId = businessId,
                PlanId = planId,
                Status = SubscriptionStatus.Active,
                StartDateUtc = now,
                EndDateUtc = now.AddDays(days)
            };
            _dbContext.TenantSubscriptions.Add(newSubscription);
            
            
            business.PlanId = plan.Id;
            business.IsActive = true;

            return await _dbContext.SaveChangesAsync() > 0;
        }

        //Remove Subscription
        public async Task<bool> DeactivateSubscription(int businessId, int planId)
        {
            var business = await _dbContext.Business
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
                throw new KeyNotFoundException($"Business with ID {businessId} was not found.");

            // Fetch the latest active (or active/trial) subscription record
            var subscription = await _dbContext.TenantSubscriptions
                 .Where(s => s.BusinessId == businessId && s.PlanId == planId
                          && s.Status == SubscriptionStatus.Active
                          && s.EndDateUtc > DateTime.UtcNow)
                 .OrderByDescending(s => s.Id)
                 .FirstOrDefaultAsync();

            if (subscription == null)
            {
                throw new SubscriptionNotFoundException();
            }

            
            
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.EndDateUtc = DateTime.UtcNow;
            business.IsActive = false;

            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
