using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();

        public bool HasUsedFreeTrial { get; private set; } = false;

        public bool CanStartFreeTrial()
        {
            // Must never have used a trial before
            if (HasUsedFreeTrial)
                return false;

            return true;
        }

        public void MarkFreeTrialUsed()
        {
            HasUsedFreeTrial = true;
        }

        public void UndoMarkFreeTrialUsed()
        {
            HasUsedFreeTrial = false;
        }

    }
}
