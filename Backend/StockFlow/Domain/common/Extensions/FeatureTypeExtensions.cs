using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Domain.common.Extensions
{
    public static class FeatureTypeExtensions
    {
        public static string ToFeatureKey(this FeatureType featureType) => featureType switch
        {
            FeatureType.MaxWarehouses => FeatureKeys.MaxWarehouses,
            FeatureType.MaxUsers => FeatureKeys.MaxUsers,
           
            _ => throw new ArgumentOutOfRangeException(nameof(featureType), featureType, $"Unmapped feature type: {featureType}")
        };
    }
}
