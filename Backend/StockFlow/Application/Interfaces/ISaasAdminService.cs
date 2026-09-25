using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.businessOwner;
using static Application.DTOs.businessOwner.BusinessOwnerDto;

namespace Application.Interfaces
{
    public interface ISaaSAdminService
    {
        Task<IEnumerable<BusinessOwnerResponseDto>> GetAllBusinessOwnersAsync();
        Task<bool> UpdateBusinessOwnerStatusAsync(int ownerId, UpdateOwnerStatusRequest request);
    }
}
