using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<CheckoutSessionResponse> SubscribeAsync(
            int businessId,
            int planId,
            string provider,
            string successUrl,
            string cancelUrl,
            CancellationToken ct = default);

        Task<TenantSubscriptionResponse?> GetCurrentSubscriptionAsync(
            int businessId,
            CancellationToken ct = default);
    }
}
