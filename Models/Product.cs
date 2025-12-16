namespace Flexi2.Models.Products
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool HasModifiers { get; set; } // за следващата стъпка
    }
}
