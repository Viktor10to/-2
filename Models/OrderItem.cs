using Flexi2.Models.Products;

namespace Flexi2.Models
{
    public class OrderItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;

        public decimal LineTotal => Product.Price * Quantity;
    }
}
