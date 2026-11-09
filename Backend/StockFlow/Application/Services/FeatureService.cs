using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.common.Extensions;
using Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IAppDbContext _dbContext;
        private readonly ILogger<FeatureService> _logger;

        public FeatureService(IAppDbContext dbContext, ILogger<FeatureService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                _logger.LogDebug("Feature limit checked. Feature: {FeatureKey}, CurrentCount: {CurrentCount}, Limit: Unlimited", featureKey, currentCount);
                return true;
            }

            if (int.TryParse(feature.Value, out int maxAllowed))
            {
                var canCreate = currentCount < maxAllowed;
                _logger.LogDebug("Feature limit checked. Feature: {FeatureKey}, CurrentCount: {CurrentCount}, MaxAllowed: {MaxAllowed}, CanCreate: {CanCreate}", featureKey, currentCount, maxAllowed, canCreate);
                return canCreate;
            }

            _logger.LogWarning("Feature limit has an unsupported value. Feature: {FeatureKey}", featureKey);
            return false;
        }
    }
}
