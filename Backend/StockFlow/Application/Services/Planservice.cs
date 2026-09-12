using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;

        public PlanService(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<List<PlanResponseDto>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetAllAsync();
            return plans.Select(MapToResponse).ToList();
        }

        public async Task<PlanResponseDto?> GetPlanByIdAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            return plan is null ? null : MapToResponse(plan);
        }

        public async Task<PlanResponseDto> CreatePlanAsync(PlanRequestDto request)
        {
            var plan = new Plan
            {
                Name = request.Name.Trim(),
                Price = request.Price,
                BillingCycle = request.BillingCycle,
                Description = request.Description.Trim(),
                IsActive = request.IsActive,
                Features = BuildFeatureList(request.Features)
            };

            await _planRepository.AddAsync(plan);
            await _planRepository.SaveChangesAsync();

            return MapToResponse(plan);
        }

        public async Task<PlanResponseDto?> UpdatePlanAsync(int id, PlanRequestDto request)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan is null)
            {
                return null;
            }

            plan.Name = request.Name.Trim();
            plan.Price = request.Price;
            plan.BillingCycle = request.BillingCycle;
            plan.Description = request.Description.Trim();
            plan.IsActive = request.IsActive;

            // Full replace rather than diffing which features were added,
           
            plan.Features.Clear();
            foreach (var feature in BuildFeatureList(request.Features))
            {
                plan.Features.Add(feature);
            }

            _planRepository.Update(plan);
            await _planRepository.SaveChangesAsync();

            return MapToResponse(plan);
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan is null)
            {
                return false;
            }

            _planRepository.Delete(plan);
            await _planRepository.SaveChangesAsync();
            return true;
        }

        private static List<PlanFeature> BuildFeatureList(List<string> featureNames)
        {
            return featureNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => new PlanFeature { Name = name.Trim() })
                .ToList();
        }

        private static PlanResponseDto MapToResponse(Plan plan)
        {
            return new PlanResponseDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Price = plan.Price,
                BillingCycle = plan.BillingCycle.ToString(),
                Description = plan.Description,
                IsActive = plan.IsActive,
                Features = plan.Features.Select(f => f.Name).ToList()
            };
        }
    }
}