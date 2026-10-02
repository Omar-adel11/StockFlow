using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Application.DTOs.Payment.Paymentdtos;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        /// <summary>
        /// Initiates a subscription checkout session for the authenticated tenant business.
        /// </summary>
        [HttpPost("checkout")]
        [Authorize]
        public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckout(
            [FromBody] CreateCheckoutSessionRequest request)
        {
            int businessId = GetCurrentBusinessId();

            var response = await _paymentService.CreateCheckoutSessionAsync(
                businessId,
                request,
                HttpContext.RequestAborted);

            return Ok(response);
        }

       

        /// <summary>
        /// Public Webhook endpoint for Paymob transaction callbacks.
        /// </summary>
        [HttpPost("webhook/paymob")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook()
        {
            // 1. Read raw request payload
            using var reader = new StreamReader(Request.Body);
            string rawBody = await reader.ReadToEndAsync();

            string hmacHeader = Request.Query["hmac"].ToString();

            // 2. Delegate HMAC verification to Paymob gateway strategy
            var paymobGateway = _gatewayFactory.GetProvider("Paymob");
            bool isValidSignature = await paymobGateway.VerifyWebhookSignatureAsync(rawBody, hmacHeader);

            if (!isValidSignature)
            {
                _logger.LogWarning("Invalid HMAC signature received for Paymob webhook.");
                return BadRequest("Invalid Paymob HMAC signature.");
            }

            // 3. Deserialize JSON callback payload
            var payload = JsonSerializer.Deserialize<PaymobCallbackPayload>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload?.Obj != null)
            {
                // Fallback sequence: Check SpecialReference (Intention API) first, then Order.MerchantOrderId (Legacy API)
                string reference = !string.IsNullOrEmpty(payload.Obj.SpecialReference)
                    ? payload.Obj.SpecialReference
                    : payload.Obj.Order?.MerchantOrderId ?? string.Empty;

                if (string.IsNullOrEmpty(reference) || !reference.StartsWith("STOCKFLOW-"))
                {
                    _logger.LogWarning("Paymob webhook payload does not contain a valid STOCKFLOW reference: {Reference}", reference);
                    return BadRequest("Invalid or missing transaction reference.");
                }

                int businessId = int.Parse(reference.Split('-')[1]);
                int planId = int.Parse(reference.Split('-')[2]);

                var callbackRequest = new ProcessPaymentCallbackRequest(
                    Provider: "Paymob",
                    ExternalTransactionId: payload.Obj.Id.ToString(),
                    Amount: payload.Obj.AmountCents / 100m,
                    IsSuccess: payload.Obj.Success,
                    BusinessId: businessId,
                    planId : planId
                );

                // 4. Delegate database update, transaction logging, and idempotency check to PaymentService
                await _paymentService.ProcessPaymentCallbackAsync(callbackRequest);
            }

            return Ok();
        }

        private int GetCurrentBusinessId()
        {
            var claim = User.FindFirst("business_id")?.Value;
            return claim != null ? int.Parse(claim) : 0;
        }
    }
}