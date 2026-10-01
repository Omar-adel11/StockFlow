using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Application.DTOs.Payment
{
    public class Paymentdtos
    {
        // Request from Frontend to start subscription checkout
        public record CreateCheckoutSessionRequest(
            int PlanId,
            string Provider, // "Paymob" or "Stripe"
            string SuccessUrl,
            string CancelUrl
        );

        // Response sent back to Frontend containing payment URL
        public record CheckoutSessionResponse(
            string PaymentUrl,
            string TransactionOrIntentionId
        );

        // Current business subscription info returned to UI
        public record TenantSubscriptionResponse(
            int Id,
            int BusinessId,
            string PlanName,
            SubscriptionStatus Status,
            DateTime StartDateUtc,
            DateTime EndDateUtc,
            bool AutoRenew
        );

        public record ProcessPaymentCallbackRequest(
            string Provider,
            string ExternalTransactionId,
            decimal Amount,
            bool IsSuccess,
            int BusinessId
        );

        // Internal DTO mapping Paymob Webhook JSON schema
        public class PaymobCallbackPayload
        {
            public PaymobTransactionObj? Obj { get; set; }
        }

        public class PaymobTransactionObj
        {
            public long Id { get; set; }
            public bool Success { get; set; }

            [JsonPropertyName("amount_cents")]
            public decimal AmountCents { get; set; }

            [JsonPropertyName("special_reference")]
            public string? SpecialReference { get; set; }

            public PaymobOrder? Order { get; set; }
        }

        public class PaymobOrder
        {
            [JsonPropertyName("merchant_order_id")]
            public string MerchantOrderId { get; set; } = string.Empty;
        }
    }
}
