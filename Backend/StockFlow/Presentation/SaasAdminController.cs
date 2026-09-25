using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.businessOwner.BusinessOwnerDto;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/saasadmin")]
    [Authorize(Roles = Roles.SaasAdmin)]
    public class SaaSAdminController : ControllerBase
    {
        private readonly ISaaSAdminService _adminService;

        public SaaSAdminController(ISaaSAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("business-owners")]
        public async Task<IActionResult> GetAllBusinessOwners()
        {
            var owners = await _adminService.GetAllBusinessOwnersAsync();
            return Ok(owners);
        }

        [HttpPut("business-owners/{id:int}/status")]
        public async Task<IActionResult> UpdateOwnerStatus(int id, [FromBody] UpdateOwnerStatusRequest request)
        {
            var result = await _adminService.UpdateBusinessOwnerStatusAsync(id, request);
            return result ? Ok(new { message = "Status updated successfully." }) : BadRequest("Failed to update status.");
        }
    }
}