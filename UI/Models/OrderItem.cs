namespace Flexi2.Models
{
    public sealed class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public bool IsLocked { get; set; } = true; // вече поръчано = locked
    }
}
