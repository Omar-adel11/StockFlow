using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class TenantSubscription
    {
        public int Id { get; set; }
        public int BusinessId { get; set; } // Identifies the tenant

        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        public SubscriptionStatus Status { get; set; } // Active, PastDue, Expired, Cancelled

        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public bool AutoRenew { get; set; } = false;

        // Metadata for payment provider mapping
        public string? ExternalSubscriptionId { get; set; }
    }
}
