using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.NotFound
{
    public class SubscriptionNotFoundException() : NotFoundException("No active subscription record found for this business.")
    {
    }
}
