using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Application.Interfaces
{
    public interface IFeatureService
    {
        Task<bool> CanCreateEntityAsync(int businessId, FeatureType featureKey, int currentCount, CancellationToken ct = default);
    }
}
