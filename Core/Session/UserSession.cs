using Flexi2.Models;

namespace Flexi2.Session
{
    public sealed class UserSession
    {
        public User? CurrentUser { get; private set; }
        public int CurrentTableId { get; set; }
        

        public bool IsLogged => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public bool IsPos => CurrentUser?.Role == UserRole.Pos;

        public void Set(User user) => CurrentUser = user;
        public void Clear() => CurrentUser = null;
    }
}
