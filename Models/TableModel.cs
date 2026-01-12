using System;

namespace Flexi2.Models
{
    public sealed class TableModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public TableStatus Status { get; set; } = TableStatus.Free;

        public decimal CurrentTotal { get; set; }
        public DateTime? OpenedAtUtc { get; set; }
        public int? OwnerUserId { get; set; }
    }
}
