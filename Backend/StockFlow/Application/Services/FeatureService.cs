using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.common.Extensions;
using Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IAppDbContext _dbContext;

        public FeatureService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CanCreateEntityAsync(int businessId, FeatureType featureKey, int currentCount, CancellationToken ct = default)
        {
            // 1. Fetch active subscription and related plan features



            var activePlanId = await _dbContext.TenantSubscriptions
                .Where(s => s.BusinessId == businessId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDateUtc > DateTime.UtcNow)
                .Select(s => s.PlanId)
                .FirstOrDefaultAsync(ct);

            if (activePlanId == 0)
            {
                // No active subscription found
                throw new InvalidOperationException("No active subscription found. please subscribe to plan");
            }

            // 2. Fetch the feature configuration for the feature key
            var feature = await _dbContext.PlanFeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.PlanId == activePlanId
                                       && f.FeatureKey == featureKey.ToFeatureKey()
                                       && f.IsActive, ct);

            if (feature == null)
            {
                // Feature key not included in active plan
                throw new InvalidOperationException($"{featureKey} not included in active plan");
            }

            // 3. Evaluate value rule ("Unlimited", numeric limit, etc.)
            if (string.Equals(feature.Value, "Unlimited", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (int.TryParse(feature.Value, out int maxAllowed))
            {
                return currentCount < maxAllowed;
            }

            return false;
        }
    }
}
