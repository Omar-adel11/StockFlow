using System.Collections.Generic;

namespace Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    }
}
