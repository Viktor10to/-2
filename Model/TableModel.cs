using System;

namespace FlexiPOS.Models
{
    public enum TableShape { Rectangle, Circle }
    public enum TableStatus { Free, Occupied }

    public sealed class TableModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int ZoneId { get; set; }

        public TableShape Shape { get; set; } = TableShape.Rectangle;

        public TableStatus Status { get; set; } = TableStatus.Free;

        public int? OwnerUserId { get; set; } // кой сервитьор я държи
        public decimal CurrentTotal { get; set; }
        public DateTime? OpenedAt { get; set; }
    }
}
