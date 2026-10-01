using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Application.DTOs.Payment.Paymentdtos;

namespace Persistence.Payments
{
    public class PaymobPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IAppDbContext _dbContext;
        private readonly ILogger<PaymobPaymentGateway> _logger;

        public string ProviderName => "Paymob";

        public PaymobPaymentGateway(HttpClient httpClient, IConfiguration config, IAppDbContext dbContext, ILogger<PaymobPaymentGateway> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
            TenantSubscription subscription,
            decimal amount,
            string currency,
            string idempotencyKey,
            string successUrl,
            string cancelUrl,
            CancellationToken ct = default)
        {
            string secretKey = _config["Paymob:SecretKey"] ?? throw new InvalidOperationException("Paymob SecretKey not configured.");
            string publicKey = _config["Paymob:PublicKey"] ?? throw new InvalidOperationException("Paymob PublicKey not configured.");
            string integrationId = _config["Paymob:IntegrationId"] ?? throw new InvalidOperationException("Paymob IntegrationId not configured.");

            if (!int.TryParse(integrationId, out int parsedIntegrationId))
            {
                throw new InvalidOperationException("Paymob IntegrationId in appsettings.json must be a valid integer.");
            }

            var businessResult = await _dbContext.Business
                .Where(b => b.Id == subscription.BusinessId)
                .Select(b => new
                {
                    Business = b,
                    Owner = (from u in b.Users
                             join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                             join r in _dbContext.Roles on ur.RoleId equals r.Id
                             where r.Name == Roles.BusinessOwner
                             select u).FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            var owner = businessResult?.Owner;

            string firstName = "NA";
            string lastName = "NA";

            if (owner?.Name is { } full)
            {
                var parts = full.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0])) firstName = parts[0];
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1])) lastName = parts[1];
            }

            var payload = new
            {
                amount = (long)Math.Round(amount * 100),
                currency = currency,
                payment_methods = new[] { parsedIntegrationId },
                special_reference = idempotencyKey,
                billing_data = new
                {
                    first_name = firstName,
                    last_name = lastName,
                    email = owner?.Email ?? "no-email@stockflow.com",
                    phone_number = owner?.PhoneNumber ?? "+201000000000"
                },
                redirection_url = successUrl
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
            request.Headers.Add("Authorization", $"Token {secretKey}");
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Paymob Intention API failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Paymob API Error ({response.StatusCode}): {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<PaymobIntentionResponse>(cancellationToken: ct);

            if (result == null || string.IsNullOrWhiteSpace(result.ClientSecret))
            {
                throw new InvalidOperationException("Failed to deserialize valid ClientSecret from Paymob response.");
            }

            string paymentUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={result.ClientSecret}";

            return new CheckoutSessionResponse(paymentUrl, result.Id ?? result.CsToken ?? string.Empty);
        }

        public Task<bool> VerifyWebhookSignatureAsync(string payloadJson, string signatureHeader)
        {
            string hmacSecret = _config["Paymob:HmacSecret"] ?? string.Empty;
            if (string.IsNullOrEmpty(hmacSecret) || string.IsNullOrEmpty(signatureHeader))
                return Task.FromResult(false);

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                // Ensure payload has the "obj" node
                if (!root.TryGetProperty("obj", out var obj))
                {
                    return Task.FromResult(false);
                }

                // Paymob HMAC requires concatenating specific transaction fields in exact lexicographical order:
                // amount_cents + created_at + currency + error_occured + has_parent_transaction + id +
                // integration_id + is_3d_secure + is_auth + is_capture + is_refunded + is_standalone_payment +
                // is_voided + order.id + owner + pending + source_data.pan + source_data.sub_type + source_data.type + success

                string amountCents = GetPropertyString(obj, "amount_cents");
                string createdAt = GetPropertyString(obj, "created_at");
                string currency = GetPropertyString(obj, "currency");
                string errorOccured = GetPropertyString(obj, "error_occured").ToLower();
                string hasParentTransaction = GetPropertyString(obj, "has_parent_transaction").ToLower();
                string id = GetPropertyString(obj, "id");
                string integrationId = GetPropertyString(obj, "integration_id");
                string is3dSecure = GetPropertyString(obj, "is_3d_secure").ToLower();
                string isAuth = GetPropertyString(obj, "is_auth").ToLower();
                string isCapture = GetPropertyString(obj, "is_capture").ToLower();
                string isRefunded = GetPropertyString(obj, "is_refunded").ToLower();
                string isStandalonePayment = GetPropertyString(obj, "is_standalone_payment").ToLower();
                string isVoided = GetPropertyString(obj, "is_voided").ToLower();

                string orderId = obj.TryGetProperty("order", out var order) ? GetPropertyString(order, "id") : "";
                string owner = GetPropertyString(obj, "owner");
                string pending = GetPropertyString(obj, "pending").ToLower();

                string pan = "";
                string subType = "";
                string type = "";
                if (obj.TryGetProperty("source_data", out var sourceData))
                {
                    pan = GetPropertyString(sourceData, "pan");
                    subType = GetPropertyString(sourceData, "sub_type");
                    type = GetPropertyString(sourceData, "type");
                }

                string success = GetPropertyString(obj, "success").ToLower();

                // Concatenate all 20 fields in exact order
                string concatenatedString = string.Concat(
                    amountCents,
                    createdAt,
                    currency,
                    errorOccured,
                    hasParentTransaction,
                    id,
                    integrationId,
                    is3dSecure,
                    isAuth,
                    isCapture,
                    isRefunded,
                    isStandalonePayment,
                    isVoided,
                    orderId,
                    owner,
                    pending,
                    pan,
                    subType,
                    type,
                    success
                );

                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
                string computedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                bool isValid = string.Equals(computedHmac, signatureHeader, StringComparison.OrdinalIgnoreCase);
                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Paymob HMAC signature validation.");
                return Task.FromResult(false);
            }
        }

        private static string GetPropertyString(System.Text.Json.JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.True => "true",
                    System.Text.Json.JsonValueKind.False => "false",
                    System.Text.Json.JsonValueKind.Null => "",
                    _ => prop.ToString() ?? ""
                };
            }
            return "";
        }
        private record PaymobIntentionResponse(
            [property: JsonPropertyName("cs_token")] string? CsToken,
            [property: JsonPropertyName("client_secret")] string? ClientSecret,
            [property: JsonPropertyName("id")] string? Id
        );
    }
}