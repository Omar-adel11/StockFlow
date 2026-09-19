using System;
using System.Collections.Generic;
using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class SalesOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Which warehouse the stock is fulfilled from - needed so
        // completing this order decrements the right InventoryItem row.
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }
}
