using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireTenantAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.Filters.Any(f => f is IAllowAnonymousFilter))
            {
                return;
            }

            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return; // Let standard [Authorize] handle 401 Unauthenticated
            }

            bool isSaasAdmin = user.IsInRole("SaasAdmin") || 
                               user.HasClaim(ClaimTypes.Role, "SaasAdmin");

            if (isSaasAdmin)
            {
                return; // SaasAdmin doesn't need a business/tenant ID
            }
            
            var businessIdClaim = user.FindFirst("business_id")?.Value;

            if (string.IsNullOrEmpty(businessIdClaim) || !int.TryParse(businessIdClaim, out int businessId) || businessId <= 0)
            {
                context.Result = new ObjectResult(new { message = "Forbidden: User context is not associated with a valid tenant business." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }


        }
    }
}
