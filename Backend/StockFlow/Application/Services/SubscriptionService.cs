using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IPaymentService _paymentService;
        private readonly IAppDbContext _dbContext;

        public SubscriptionService(IPaymentService paymentService, IAppDbContext dbContext)
        {
            _paymentService = paymentService;
            _dbContext = dbContext;
        }

        public async Task<CheckoutSessionResponse> SubscribeAsync(
            int businessId,
            int planId,
            string provider,
            string successUrl,
            string cancelUrl,
            CancellationToken ct = default)
        {
            // 1. Verify business exists
            var businessExists = await _dbContext.Business.IgnoreQueryFilters()
                .AnyAsync(b => b.Id == businessId, ct);

            if (!businessExists)
            {
                throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
            }

            // 2. Delegate checkout session creation to PaymentService
            var request = new CreateCheckoutSessionRequest(
                PlanId: planId,
                Provider: provider,
                SuccessUrl: successUrl,
                CancelUrl: cancelUrl
            );

            return await _paymentService.CreateCheckoutSessionAsync(businessId, request, ct);
        }

       

        public async Task<TenantSubscriptionResponse?> GetCurrentSubscriptionAsync(
           int businessId,
           CancellationToken ct = default)
        {
            var subs =  await _dbContext.TenantSubscriptions
                .Where(s => s.BusinessId == businessId)
                .Select(s => new TenantSubscriptionResponse(
                    s.Id,
                    s.BusinessId,
                    s.Plan.Name,
                    s.Status,
                    s.StartDateUtc,
                    s.EndDateUtc,
                    s.AutoRenew))
                .FirstOrDefaultAsync(ct);
            if (subs == null)
            {
                throw new SubscriptionNotFoundException();
            }
            return subs;
        }
    }
}
