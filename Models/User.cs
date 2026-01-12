using System;

namespace Flexi2.Models
{
    public sealed class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.Pos;

        public string PinHash { get; set; } = "";
        public string PinSalt { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
