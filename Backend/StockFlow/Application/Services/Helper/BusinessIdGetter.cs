using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Auth;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Helper
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetBusinessId(this ClaimsPrincipal? user)
        {
            var claimValue = user?.FindFirst(CustomClaimTypes.BusinessId)?.Value;
            return int.TryParse(claimValue, out var businessId) ? businessId : 0;
        }
    }
}
