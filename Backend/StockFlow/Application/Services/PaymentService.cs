using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAppDbContext _dbContext;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IAppDbContext dbContext,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<PaymentService> logger)
        {
            _dbContext = dbContext;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
            int businessId,
            CreateCheckoutSessionRequest request,
            CancellationToken ct = default)
        {
            var plan = await _dbContext.Plans
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, ct);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Active plan with ID {request.PlanId} was not found.");
            }

            var subscription = await _dbContext.TenantSubscriptions
                .FirstOrDefaultAsync(s => s.BusinessId == businessId, ct);

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
                subscription.PlanId = plan.Id;
                subscription.Plan = plan;
            }

            string idempotencyKey = $"STOCKFLOW-{businessId}-{plan.Id}-{DateTime.UtcNow:yyyyMMddHHmm}";
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

        public async Task<bool> ProcessPaymentCallbackAsync(
            ProcessPaymentCallbackRequest request,
            CancellationToken ct = default)
        {
            // 1. Idempotency Check: Guard against duplicate webhook processing
            bool isProcessed = await _dbContext.PaymentTransactions
                .AnyAsync(t => t.ExternalTransactionId == request.ExternalTransactionId && t.IsProcessed, ct);

            if (isProcessed)
            {
                _logger.LogInformation("Transaction {TransactionId} has already been processed.", request.ExternalTransactionId);
                return true;
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var dbTx = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var subscription = await _dbContext.TenantSubscriptions
                        .FirstOrDefaultAsync(s => s.BusinessId == request.BusinessId, ct);

                    if (request.IsSuccess)
                    {
                        if (subscription == null)
                        {
                            subscription = new TenantSubscription
                            {
                                BusinessId = request.BusinessId,
                                PlanId = 1,
                                Status = SubscriptionStatus.Active,
                                StartDateUtc = DateTime.UtcNow,
                                EndDateUtc = DateTime.UtcNow.AddMonths(1)
                            };
                            _dbContext.TenantSubscriptions.Add(subscription);
                        }
                        else
                        {
                            subscription.Status = SubscriptionStatus.Active;

                            // Extend from active balance or reset from today
                            DateTime baseDate = subscription.EndDateUtc > DateTime.UtcNow
                                ? subscription.EndDateUtc
                                : DateTime.UtcNow;

                            subscription.StartDateUtc = DateTime.UtcNow;
                            subscription.EndDateUtc = baseDate.AddMonths(1);
                        }
                    }

                    var transactionRecord = new PaymentTransaction
                    {
                        BusinessId = request.BusinessId,
                        TenantSubscription = subscription,
                        Provider = request.Provider,
                        ExternalTransactionId = request.ExternalTransactionId,
                        Amount = request.Amount,
                        Status = request.IsSuccess ? MyTransactionStatus.Success : MyTransactionStatus.Failed,
                        IsProcessed = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        ProcessedAtUtc = DateTime.UtcNow
                    };

                    _dbContext.PaymentTransactions.Add(transactionRecord);
                    await _dbContext.SaveChangesAsync(ct);
                    await dbTx.CommitAsync(ct);

                    return true;
                }
                catch (Exception ex)
                {
                    await dbTx.RollbackAsync(ct);
                    _logger.LogError(ex, "Error processing payment callback for transaction {TransactionId}", request.ExternalTransactionId);
                    return false;
                }
            });
        }

        public async Task<TenantSubscriptionResponse?> GetCurrentSubscriptionAsync(
            int businessId,
            CancellationToken ct = default)
        {
            return await _dbContext.TenantSubscriptions
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
        }
    }
}
