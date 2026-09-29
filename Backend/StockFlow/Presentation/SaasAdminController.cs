using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
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
        private readonly IBusinessService _businessService;

        public SaaSAdminController(ISaaSAdminService adminService, IBusinessService businessService)
        {
            _adminService = adminService;
            _businessService = businessService;
        }

        [HttpGet("business-owners")]
        public async Task<IActionResult> GetAllBusinessOwners([FromQuery] string? search)
        {
            var owners = await _adminService.GetAllBusinessOwnersAsync(search);
            return Ok(owners);
        }

        [HttpPut("business-owners/{id:int}/status")]
        public async Task<IActionResult> UpdateOwnerStatus(int id, [FromBody] UpdateOwnerStatusRequest request)
        {
            var result = await _adminService.UpdateBusinessOwnerStatusAsync(id, request);
            return result ? Ok(new { message = "Status updated successfully." }) : BadRequest("Failed to update status.");
        }

        //Assign plan
        [HttpPut("businesses/{businessId}/plan/{planId}")]
        public async Task<IActionResult> AssignPlan(int businessId, int planId)
        {
            await _businessService.AssignPlanAsync(businessId, planId);
            return Ok(new { message = "Plan assigned successfully to business." });
        }
    }
}