namespace Flexi2.Models
{
    public class Product
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }

        // МНОГО ВАЖНО – това липсваше
        public string Category { get; set; } = "";
    }
}
