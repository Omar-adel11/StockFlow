using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int? TenantSubscriptionId { get; set; }
        public TenantSubscription? TenantSubscription { get; set; }

        public string Provider { get; set; } = string.Empty; // "Paymob" or "Stripe"
        public string ExternalTransactionId { get; set; } = string.Empty; // Paymob Transaction ID or Stripe Intent ID

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";

        public MyTransactionStatus Status { get; set; } // Pending, Success, Failed
        public bool IsProcessed { get; set; } // Idempotency flag

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAtUtc { get; set; }
    }
}
