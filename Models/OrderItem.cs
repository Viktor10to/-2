namespace Flexi2.Models
{
    public class OrderItem
    {
        public Product Product { get; set; } = null!;
        public int Qty { get; set; }
        public bool IsLocked { get; set; }  // след "Поръчай"
        public decimal Total => Product.Price * Qty;
    }
}
