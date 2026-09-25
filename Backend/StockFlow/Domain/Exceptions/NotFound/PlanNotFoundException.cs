using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.NotFound
{
    public class PlanNotFoundException() : NotFoundException("Plan is not found")
    {
    }
}
