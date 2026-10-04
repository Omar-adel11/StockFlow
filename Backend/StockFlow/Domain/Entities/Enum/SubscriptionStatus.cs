using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enum
{
    public enum SubscriptionStatus
    {
        // Active, PastDue, Expired, Cancelled
        Pending = 0,
        Active = 1,
        PastDue = 2,
        Expired = 3,
        Cancelled = 4,
        Trialing = 5
    }
}
