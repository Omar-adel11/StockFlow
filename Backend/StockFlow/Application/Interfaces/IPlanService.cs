using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanResponseDto>> GetAllPlansAsync();

        Task<PlanResponseDto?> GetPlanByIdAsync(int id);

        Task<PlanResponseDto> CreatePlanAsync(PlanRequestDto request);

        Task<PlanResponseDto?> UpdatePlanAsync(int id, PlanRequestDto request);

        Task<bool> DeletePlanAsync(int id);
    }
}
