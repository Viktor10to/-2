using System;

namespace Flexi2.Models
{
    public sealed class Order
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime OpenedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public decimal DiscountPercent { get; set; }
        public string PaidMethod { get; set; } = ""; // Cash/Card later
    }
}
