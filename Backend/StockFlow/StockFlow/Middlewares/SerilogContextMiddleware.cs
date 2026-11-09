using System.Security.Claims;
using Domain.Entities;
using Serilog;
using Serilog.Context;

namespace StockFlow.Middlewares
{
    public class SerilogContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;
        public SerilogContextMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _diagnosticContext = diagnosticContext;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Get authenticated user's ID.
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 2. Get BusinessId claim.
            var businessIdClaim = context.User.FindFirst("business_id")?.Value;

            // 	Adds properties to the HTTP request completion event.
            _diagnosticContext.Set("UserId", userIdClaim ?? "Anonymous");
            _diagnosticContext.Set("BusinessId", businessIdClaim ?? "None");

            // 3. Push UserId into LogContext if available.
            using (LogContext.PushProperty("UserId", userIdClaim ?? "Anonymous"))

            // 4. Push BusinessId into LogContext if available.
            using (LogContext.PushProperty("BusinessId", businessIdClaim ?? "None"))
            {
                // 5. Call the next middleware.
                await _next(context);
            }

        }
    }
}
