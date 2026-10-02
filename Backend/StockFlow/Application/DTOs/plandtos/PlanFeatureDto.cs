using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.plandtos
{
    public class PlanFeatureDto
    {
        [Required(ErrorMessage = "Feature Key is required.")]
        public string FeatureKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Feature Value is required.")]
        public string Value { get; set; } = string.Empty; // e.g. "3", "100", "Unlimited", "true"
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
