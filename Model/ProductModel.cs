namespace FlexiPOS.Models
{
    public sealed class ProductModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public bool HasModifiers { get; set; } = false;
    }
}
