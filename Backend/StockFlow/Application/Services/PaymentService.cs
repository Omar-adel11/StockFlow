using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Application.DTOs.businessOwner.BusinessOwnerDto;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAppDbContext _dbContext;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<PaymentService> _logger;
        private readonly ISubscriptionService _subscriptionService;


        public PaymentService(
            IAppDbContext dbContext,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<PaymentService> logger,
            ISubscriptionService subscriptionService)
        {
            _dbContext = dbContext;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(int businessId,CreateCheckoutSessionRequest request, CancellationToken ct = default)
        {

            var business = await _dbContext.Business.IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == businessId, ct)
                ?? throw new KeyNotFoundException($"Business with ID {businessId} was not found.");

            var plan = await _dbContext.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, ct);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Active plan with ID {request.PlanId} was not found.");
            }

            DateTime now = DateTime.UtcNow;


            var currentActiveSub = await _dbContext.TenantSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == businessId
                         && s.Status == SubscriptionStatus.Active
                         && s.EndDateUtc > now)
                .FirstOrDefaultAsync(ct);

            if (currentActiveSub != null && (currentActiveSub.Plan == null || !currentActiveSub.Plan.IsFreeTrial))
            {
                throw new InvalidOperationException("You already have an active paid subscription. Please cancel your current subscription before purchasing a new plan.");
            }

            string idempotencyKey = $"STOCKFLOW-{businessId}-{plan.Id}-{DateTime.UtcNow:yyyyMMddHHmm}";


            // 4. Record Pending Payment Transaction in DB
            var pendingTransaction = new PaymentTransaction
            {
                BusinessId = businessId,
                ExternalTransactionId = idempotencyKey, // Used as lookup key during webhook
                Amount = plan.Price,
                Provider = request.Provider,
                Status = MyTransactionStatus.Pending, // Represents Pending
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.PaymentTransactions.Add(pendingTransaction);
            await _dbContext.SaveChangesAsync(ct);


            int extensionDays = plan.BillingCycle == 0 ? 30 : 365;

            var checkoutSubscription = new TenantSubscription
            {
                BusinessId = businessId,
                PlanId = request.PlanId,
                StartDateUtc = now,
                EndDateUtc = now.AddDays(extensionDays),
                Status = SubscriptionStatus.Pending,
                AutoRenew = request.AutoRenew ?? false
            };
            

            var gateway = _gatewayFactory.GetProvider(request.Provider);

            return await gateway.CreateCheckoutSessionAsync(
                    checkoutSubscription,
                    plan.Price,
                    plan.Currency,
                    idempotencyKey,
                    request.SuccessUrl,
                    request.CancelUrl,
                    ct);

        }

        public async Task ProcessPaymentCallbackAsync(ProcessPaymentCallbackRequest request, CancellationToken ct)
        {
            // 1. Locate Pending Transaction by Reference String or Gateway Transaction ID
            var pendingTransaction = await _dbContext.PaymentTransactions
                .FirstOrDefaultAsync(t => t.ExternalTransactionId == request.ExternalTransactionId, ct);

            if (!request.IsSuccess)
            {
                _logger.LogWarning("Payment failed for transaction ref: {Ref}", request.ExternalTransactionId);
                if (pendingTransaction != null)
                {
                    pendingTransaction.Status = MyTransactionStatus.Failed;
                    await _dbContext.SaveChangesAsync(ct);
                }
                return;
            }

            // 2. Idempotency Check: Skip if transaction was already finalized
            if (pendingTransaction != null && pendingTransaction.Status == MyTransactionStatus.Success)
            {
                _logger.LogInformation("Transaction {Ref} has already been processed.", request.ExternalTransactionId);
                return;
            }

            // 3. Update or Create Tenant Subscription

            var plan = await _dbContext.Plans.FirstOrDefaultAsync(p => p.Id == request.planId);

            if (plan is null)
            {
                throw new PlanNotFoundException();
            }

            DateTime now = DateTime.UtcNow;

            var result = await _subscriptionService.ActivateSubscription(request.BusinessId,request.planId, plan.BillingCycle == 0 ? 30 : 365);
            
            if(result)
            {
                // 4. Mark Pending Transaction as Successful
                if (pendingTransaction != null)
                {
                    pendingTransaction.Status = MyTransactionStatus.Success;
                    pendingTransaction.Amount = request.Amount;
                }
                else
                {
                    _dbContext.PaymentTransactions.Add(new PaymentTransaction
                    {
                        BusinessId = request.BusinessId,
                        ExternalTransactionId = request.ExternalTransactionId,
                        Amount = request.Amount,
                        Provider = request.Provider,
                        Status = MyTransactionStatus.Success,
                        CreatedAtUtc = now
                    });
                }

                await _dbContext.SaveChangesAsync(ct);

              
                }
                else
                {
                    _logger.LogError("Failed to activate subscription after payment success for Business {BusinessId}", request.BusinessId);
                    throw new InvalidOperationException("Failed to activate subscription after payment success.");
                }

        }

       
    }
}
