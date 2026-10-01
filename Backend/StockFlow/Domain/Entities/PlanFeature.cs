using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.common;

namespace Domain.Entities
{
    public class PlanFeature : ISoftDelete
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        // Descriptive display name for UI (e.g. "Max Warehouses")
        public string Name { get; set; } = null!;

        // Machine-readable key for code checks (e.g. "MAX_WAREHOUSES", "EXPORT_PDF")
        public string FeatureKey { get; set; } = null!;

        // Limit value (e.g. "3", "1000", "Unlimited", "true")
        public string Value { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
