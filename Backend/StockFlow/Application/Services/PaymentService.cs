using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
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
        private readonly ISaaSAdminService _saaSAdminService;


        public PaymentService(
            IAppDbContext dbContext,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<PaymentService> logger,
            ISaaSAdminService saaSAdminService)
        {
            _dbContext = dbContext;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
            _saaSAdminService = saaSAdminService;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(int businessId,CreateCheckoutSessionRequest request, CancellationToken ct = default)
        {
            var plan = await _dbContext.Plans.FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, ct);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Active plan with ID {request.PlanId} was not found.");
            }


            var subscription = await _dbContext.TenantSubscriptions.FirstOrDefaultAsync(s => s.BusinessId == businessId, ct);

            


            if (subscription == null)
            {
                subscription = new TenantSubscription
                {
                    BusinessId = businessId,
                    PlanId = plan.Id,
                    Plan = plan,
                    Status = SubscriptionStatus.Expired,
                    StartDateUtc = DateTime.UtcNow,
                    EndDateUtc = DateTime.UtcNow
                };
            }
            else
            {
                if (subscription.PlanId == request.PlanId
                && subscription.Status == SubscriptionStatus.Active
                && subscription.EndDateUtc > DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Your business is already subscribed to this active plan.");
                }
                subscription.PlanId = plan.Id;
                subscription.Plan = plan;
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

            var gateway = _gatewayFactory.GetProvider(request.Provider);

            return await gateway.CreateCheckoutSessionAsync(
                subscription,
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
            var subscription = await _dbContext.TenantSubscriptions
                .FirstOrDefaultAsync(s => s.BusinessId == request.BusinessId, ct);

            DateTime now = DateTime.UtcNow;

            if (subscription == null)
            {
                subscription = new TenantSubscription
                {
                    BusinessId = request.BusinessId,
                    PlanId = request.planId,
                    Status = SubscriptionStatus.Active,
                    StartDateUtc = now,
                    EndDateUtc = now.AddDays(30)
                };
                _dbContext.TenantSubscriptions.Add(subscription);
            }
            else
            {
                // Stacking logic: extend from current EndDateUtc if currently active
                DateTime baseDate = (subscription.Status == SubscriptionStatus.Active && subscription.EndDateUtc > now)
                    ? subscription.EndDateUtc
                    : now;

                subscription.PlanId = request.planId;
                subscription.Status = SubscriptionStatus.Active;
                subscription.StartDateUtc = now;
                subscription.EndDateUtc = baseDate.AddDays(30);
            }

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

            // 5. Sync Business Owner Status
            var owner = await _dbContext.Users
                .Where(u => u.BusinessId == request.BusinessId
                         && _dbContext.UserRoles.Any(ur => ur.UserId == u.Id
                         && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == Roles.BusinessOwner)))
                .Select(u => new { u.Id })
                .FirstOrDefaultAsync(ct);

            if (owner != null)
            {
                var updateOwnerRequest = new UpdateOwnerStatusRequest
                {
                    IsActive = true,
                    PlanId = request.planId
                };

                bool result = await _saaSAdminService.UpdateBusinessOwnerStatusAsync(owner.Id, updateOwnerRequest);
                if (!result)
                {
                    _logger.LogError("Failed to update user plan for owner {OwnerId}", owner.Id);
                    throw new InvalidOperationException("Failed to update business owner plan.");
                }
            }

            // 6. Commit Database Transaction
            await _dbContext.SaveChangesAsync(ct);
        }

       
    }
}
