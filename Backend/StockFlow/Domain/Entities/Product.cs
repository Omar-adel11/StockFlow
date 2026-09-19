using System.Collections.Generic;

namespace Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Optional: a product can exist with no preferred supplier yet
        // (nullable FK), even though a purchase order still requires
        // choosing a specific supplier at order time.
        public int? PreferredSupplierId { get; set; }
        public Supplier? PreferredSupplier { get; set; }

        public decimal UnitPrice { get; set; }

        // Drives the dashboard's Low Stock Alert - when a w    arehouse's
        // QuantityOnHand for this product drops to or below this number.
        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
        public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
