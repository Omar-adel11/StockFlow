namespace Domain.Entities
{
    // One row per (Product, Warehouse) pair - see
    // InventoryItemConfiguration for the unique index enforcing that.
    public class InventoryItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int QuantityOnHand { get; set; }
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
