using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Services;
using Application.Services.Auth;
using Domain.Helpers;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpPost("invite")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]

        public async Task<IActionResult> InviteTeammate([FromBody] SendInviteRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var businessId = int.Parse(User.FindFirstValue(CustomClaimTypes.BusinessId)!);

            await _teamService.SendInviteAsync(request, userId, businessId);
            return Ok(new { message = "Invitation sent successfully." });
        }

        [HttpGet("invite/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInviteDetails(string token)
        {
            var result = await _teamService.GetInviteDetailsAsync(token);
            if (!result.IsValid)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("accept-invite")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvite([FromForm] AcceptInviteRequest request)
        {
            var response = await _teamService.AcceptInviteAsync(request);
            return Ok(response);
        }

        [HttpGet("invites")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> GetPendingInvites()
        {
            var businessId = int.Parse(User.FindFirstValue("business_id")!);
            var invites = await _teamService.GetPendingInvitesAsync(businessId);
            return Ok(invites);
        }

        [HttpDelete("invite/{id:int}")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> CancelInvite(int id)
        {
            var businessId = int.Parse(User.FindFirstValue("business_id")!);
            await _teamService.CancelInviteAsync(id, businessId);
            return Ok(new { message = "Invitation revoked successfully." });
        }

        [HttpGet("members")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> GetTeamMembers()
        {
            var businessId = int.Parse(User.FindFirstValue(CustomClaimTypes.BusinessId)!);
            var members = await _teamService.GetTeamMembersAsync(businessId);
            return Ok(members);
        }

        [HttpGet("members/{id:int}")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> GetMemberById(int id)
        {
            var businessId = int.Parse(User.FindFirstValue(CustomClaimTypes.BusinessId)!);
            var member = await _teamService.GetMemberByIdAsync(id, businessId);
            if (member == null) return NotFound();
            return Ok(member);
        }

        [HttpPut("members/{id:int}/role")]
        [Authorize(Roles = Roles.BusinessOwner)]
        public async Task<IActionResult> UpdateMemberRole(int id, [FromBody] UpdateRoleRequest request)
        {
            var businessId = int.Parse(User.FindFirstValue(CustomClaimTypes.BusinessId)!);
            await _teamService.UpdateMemberRoleAsync(id, request.NewRole, businessId);
            return Ok(new { message = "User role updated successfully." });
        }

        [HttpDelete("members/{id:int}")]
        [Authorize(Roles = Roles.BusinessOwner)]
        public async Task<IActionResult> RemoveMember(int id)
        {
            var businessId = int.Parse(User.FindFirstValue(CustomClaimTypes.BusinessId)!);
            await _teamService.RemoveMemberAsync(id, businessId);
            return Ok(new { message = "Team member removed successfully." });
        }
    }
}