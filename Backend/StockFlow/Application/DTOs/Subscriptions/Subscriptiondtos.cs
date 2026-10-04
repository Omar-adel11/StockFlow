using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Subscriptions
{
    public class Subscriptiondtos
    {
        public class SubscriptionDto
        {
            public int Id { get; set; }
            public int BusinessId { get; set; }
            public int PlanId { get; set; }
            public string PlanName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty; // e.g., "Trialing", "Active"
            public DateTime StartDateUtc { get; set; }
            public DateTime EndDateUtc { get; set; }
            public bool IsActive { get; set; }
        }

      

    }
}

