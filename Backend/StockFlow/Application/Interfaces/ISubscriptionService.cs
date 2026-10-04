using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.Payment.Paymentdtos;
using static Application.DTOs.Subscriptions.Subscriptiondtos;

namespace Application.Interfaces
{
    public interface ISubscriptionService
    {
        

        Task<TenantSubscriptionResponse?> GetCurrentSubscriptionAsync(
            int businessId,
            CancellationToken ct = default);
        Task<SubscriptionDto> StartFreeTrialAsync(
        int businessId,
        int planId,
        CancellationToken ct = default);

        Task<bool> DeactivateSubscription(int businessId, int planId);
        Task<bool> ActivateSubscription(int businessId, int planId,int days);
    }   
}

