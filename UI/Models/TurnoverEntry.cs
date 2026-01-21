using System;

namespace Flexi2.Models
{
    public sealed class TurnoverEntry
    {
        public int Id { get; set; }
        public DateTime AtUtc { get; set; }
        public int UserId { get; set; }
        public int TableId { get; set; }
        public decimal TotalAfterDiscount { get; set; }
    }
}
