using System.Collections.Generic;
using Domain.common;

namespace Domain.Entities
{
    public class Supplier : ISoftDelete
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;

        public string ContactPhone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Products that name this supplier as their preferred one -
        // not every purchase order for this supplier, just the reorder default.
        public ICollection<Product> PreferredByProducts { get; set; } = new List<Product>();

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
