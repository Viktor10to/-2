namespace Flexi2.Models
{
    public sealed class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PinHash { get; set; } = "";
        public string PinSalt { get; set; } = "";
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
