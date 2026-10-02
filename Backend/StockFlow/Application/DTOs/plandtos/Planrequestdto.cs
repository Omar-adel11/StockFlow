using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enum;

namespace Application.DTOs.plandtos
{
    // Used for both POST (create) and PUT (update) - same shape either way.

    public class PlanRequestDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "BillingCycle is required.")]
        public BillingCycle BillingCycle { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Structured feature list mapping directly to PlanFeature rows
        public List<PlanFeatureDto> Features { get; set; } = new();
    }
}