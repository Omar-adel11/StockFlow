namespace Domain.Entities
{
    public class SalesOrderItem
    {
        public int Id { get; set; }

        public int SalesOrderId { get; set; }
        public SalesOrder SalesOrder { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // The price charged to the customer at the time of sale - kept
        // separate from Product.UnitPrice in case that price changes later.
        public decimal UnitPrice { get; set; }
        public int? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
