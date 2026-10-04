using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.plandtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if(request.IsFreeTrial)
            {
                if (request.IsFreeTrial && request.Price > 0)
                {
                    throw new InvalidOperationException("A free trial plan must have a price of 0.");
                }

                //  Ensure only one active Free Trial plan exists in the database

                bool hasExistingFreeTrial = await _planRepository.AnyFreeTrialPlanAsync();

                if (hasExistingFreeTrial)
                {
                    throw new InvalidOperationException("An active Free Trial plan already exists.");
                }
                
            }
           

            var plan = new Plan
            {
                Name = request.Name.Trim(),
                Price = request.Price,
                BillingCycle = request.BillingCycle,
                Description = request.Description.Trim(),
                IsActive = request.IsActive,
                Features = BuildFeatureList(request.Features),
                IsFreeTrial = request.IsFreeTrial
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

            // Full replace strategy for aggregate root child collection
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
                throw new PlanNotFoundException();
            }

            _planRepository.Delete(plan);
            await _planRepository.SaveChangesAsync();
            return true;
        }

        private static List<PlanFeature> BuildFeatureList(List<PlanFeatureDto> featureDtos)
        {
            if (featureDtos == null || !featureDtos.Any())
                return new List<PlanFeature>();

            return featureDtos
                .Where(dto => !string.IsNullOrWhiteSpace(dto.FeatureKey))
                .Select(dto => new PlanFeature
                {
                    FeatureKey = dto.FeatureKey.Trim().ToUpperInvariant(),
                    Value = dto.Value?.Trim() ?? string.Empty,
                    Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.FeatureKey.Trim() : dto.Name.Trim(),
                    IsActive = dto.IsActive
                })
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
                IsFreeTrial = plan.IsFreeTrial,
                Features = plan.Features?.Select(f => new PlanFeatureDto
                {
                    Name = f.Name,
                    FeatureKey = f.FeatureKey,
                    Value = f.Value,
                    IsActive = f.IsActive
                }).ToList() ?? new List<PlanFeatureDto>()
            };
        }
    }
}