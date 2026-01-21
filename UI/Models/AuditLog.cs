using System;

namespace Flexi2.Models
{
    public sealed class AuditLog
    {
        public int Id { get; set; }
        public DateTime AtUtc { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = "";
        public string Entity { get; set; } = "";
        public int EntityId { get; set; }
        public string Details { get; set; } = "";
    }
}
