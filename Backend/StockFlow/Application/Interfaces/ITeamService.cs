using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Team;
using Application.DTOs.userDtos;

namespace Application.Interfaces
{
    public interface ITeamService
    {
        Task SendInviteAsync(SendInviteRequest request, int currentUserId, int currentBusinessId);
        Task<InviteDetailsResponse> GetInviteDetailsAsync(string token);
        Task<UserDTO> AcceptInviteAsync(AcceptInviteRequest request);
        Task CancelInviteAsync(int inviteId, int currentBusinessId);
        Task<List<TeamInvitationDto>> GetPendingInvitesAsync(int currentBusinessId);

        Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int businessId);
        Task<TeamMemberDto?> GetMemberByIdAsync(int memberId, int businessId);
        Task UpdateMemberRoleAsync(int memberId, string newRole, int businessId);
        Task RemoveMemberAsync(int memberId, int businessId);
    }
}
