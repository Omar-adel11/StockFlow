namespace Domain.Entities
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // The cost paid to the supplier - kept separate from
        // Product.UnitPrice (the sale price) since purchase cost can
        // vary order to order.
        public decimal UnitCost { get; set; }
    }
}
