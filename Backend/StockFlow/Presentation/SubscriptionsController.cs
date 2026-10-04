using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Payment.Paymentdtos;
using static Application.DTOs.Subscriptions.Subscriptiondtos;

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
        /// Starts a 14-day free trial for eligible businesses.
        /// </summary>
        [HttpPost("start-trial")]
        public async Task<ActionResult<SubscriptionDto>> StartTrial(
            [FromQuery] int planId,
            CancellationToken ct)
        {
            int businessId = GetCurrentBusinessId();
            var result = await _subscriptionService.StartFreeTrialAsync(
                businessId,
                planId,
                ct);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves active subscription details for the authenticated business owner.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<TenantSubscriptionResponse>> GetMySubscription(CancellationToken ct)
        {
            int businessId = GetCurrentBusinessId();

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(
                businessId,
                ct);

            return Ok(subscription);
        }

        /// <summary>
        /// Cancels/deactivates the active subscription for the authenticated business owner.
        /// </summary>
        [HttpPost("cancel")]
        public async Task<ActionResult<bool>> DeactivateSubscription(
            [FromQuery] int planId,
            CancellationToken ct)
        {
            int businessId = GetCurrentBusinessId();

            bool result = await _subscriptionService.DeactivateSubscription(
                businessId,
                planId);

            return Ok(result);
        }

        private int GetCurrentBusinessId()
        {
            var claim = User.FindFirst("business_id")?.Value;
            return claim != null ? int.Parse(claim) : 0;
        }
    }
}