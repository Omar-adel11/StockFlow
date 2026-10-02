using System.Collections.Generic;

namespace Application.DTOs.plandtos
{
    public class PlanResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BillingCycle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public List<PlanFeatureDto> Features { get; set; } = new();
    }
}