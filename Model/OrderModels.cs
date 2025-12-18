using System;

namespace FlexiPOS.Models
{
    public sealed class OrderLine
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }

        public bool IsLocked { get; set; } // true = вече поръчано (read-only)
        public string? NoteOrModifiers { get; set; }

        public decimal Total => UnitPrice * Qty;
    }

    public sealed class TableOrderState
    {
        public int TableId { get; set; }
        public int? OwnerUserId { get; set; }
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        public decimal DiscountPercent { get; set; } = 0m;

        // Locked + Draft
        public System.Collections.Generic.List<OrderLine> Lines { get; set; } = new();
    }
}
