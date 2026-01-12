namespace Flexi2.Models
{
    public sealed class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }
}
