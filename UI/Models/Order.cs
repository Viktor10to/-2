using System;

namespace Flexi2.Models
{
    public sealed class Order
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsClosed { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
