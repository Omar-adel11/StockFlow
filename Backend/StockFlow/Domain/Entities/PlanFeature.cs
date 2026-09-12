using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PlanFeature
    {
        public int Id { get; set; }

        public int PlanId { get; set; }

        public string Name { get; set; } = null!;

        public Plan Plan { get; set; } = null!;
    }
}
