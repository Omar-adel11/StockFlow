using System;
using System.Collections.Generic;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDate { get; set; }

        // Plain FK to AspNetUsers.Id - no navigation property, since that
        // would require adding a collection property on your existing
        // User (Identity) class, which isn't touched here.
        public int CreatedByUserId { get; set; }

        // Kept as a stored, calculated-on-save total rather than always
        // summing PurchaseOrderItems on read - cheaper for list views,
        // at the cost of needing to recompute it whenever items change.
        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
