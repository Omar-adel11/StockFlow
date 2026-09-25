using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.businessOwner
{
    public class BusinessOwnerDto
    {
        public class BusinessOwnerResponseDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public int? BusinessId { get; set; }
            public string? BusinessName { get; set; }
            public int? CurrentPlanId { get; set; }
            public string? CurrentPlanName { get; set; }
        }
        public class UpdateOwnerStatusRequest
        {
            public bool IsActive { get; set; }
            public int? PlanId { get; set; }
        }
    }
}
