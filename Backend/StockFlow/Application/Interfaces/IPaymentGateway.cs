using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Interfaces
{
    public interface IPaymentGateway
    {
        string ProviderName { get; } // e.g., "Paymob" or "Stripe"

        Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
            TenantSubscription subscription,
            decimal amount,
            string currency,
            string idempotencyKey,
            string successUrl,
            string cancelUrl,
            CancellationToken ct = default);

        Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader);
    }
}
