using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions.NotFound;

namespace Application.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IPlanRepository _planRepository;

        public BusinessService(
            IBusinessRepository businessRepository,
            IPlanRepository planRepository)
        {
            _businessRepository = businessRepository;
            _planRepository = planRepository;
        }

        public async Task<bool> AssignPlanAsync(int businessId, int planId)
        {
            // 1. Validate Business Exists
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
            {
                throw new BusinessNotFoundException();
            }

            // 2. Validate Plan Exists & Is Active
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null)
            {
                throw new PlanNotFoundException();
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException("Cannot assign an inactive plan to a business.");
            }

            // 3. Assign Plan to Business & Persist
            business.PlanId = plan.Id;

            _businessRepository.Update(business);
            await _businessRepository.SaveChangesAsync();

            return true;
        }
    }
}
