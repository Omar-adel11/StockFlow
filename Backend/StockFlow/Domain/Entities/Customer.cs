using System.Collections.Generic;
using Domain.common;

namespace Domain.Entities
{
    public class Customer : ISoftDelete
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
        public bool IsActive { get ; set ; }
    }
}
