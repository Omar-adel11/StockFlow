using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Payment.Paymentdtos;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Initiates a subscription flow for the authenticated business owner.
        /// Redirects to the payment gateway checkout.
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<ActionResult<CheckoutSessionResponse>> Subscribe(
            [FromBody] CreateCheckoutSessionRequest request)
        {
            int businessId = GetCurrentBusinessId();

            var response = await _subscriptionService.SubscribeAsync(
                businessId: businessId,
                planId: request.PlanId,
                provider: request.Provider,
                successUrl: request.SuccessUrl,
                cancelUrl: request.CancelUrl,
                ct: HttpContext.RequestAborted);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the active subscription details for the current tenant business.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<TenantSubscriptionResponse>> GetMySubscription()
        {
            int businessId = GetCurrentBusinessId();

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(
                businessId,
                HttpContext.RequestAborted);


            return Ok(subscription);
        }

        private int GetCurrentBusinessId()
        {
            var claim = User.FindFirst("business_id")?.Value;
            return claim != null ? int.Parse(claim) : 0;
        }
    }
}