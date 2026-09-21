using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Persistence
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public string UserId =>
            httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

        public string UserName =>
            httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
            ?? httpContextAccessor.HttpContext?.User?.Identity?.Name
            ?? "System Process";
    }
}
