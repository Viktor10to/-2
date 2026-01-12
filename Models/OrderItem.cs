using System;

namespace Flexi2.Models
{
    public sealed class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string NameSnapshot { get; set; } = "";
        public decimal PriceSnapshot { get; set; }
        public int Qty { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
