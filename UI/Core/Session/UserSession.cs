using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.Core.Session
{
    public sealed class UserSession : ObservableObject
    {
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            private set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLoggedIn)); }
        }

        private int? _currentTableId;
        public int? CurrentTableId
        {
            get => _currentTableId;
            set { _currentTableId = value; OnPropertyChanged(); }
        }

        public bool IsLoggedIn => CurrentUser != null;

        public void SetUser(User user)
        {
            CurrentUser = user;
            CurrentTableId = null;
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentTableId = null;
        }
    }
}
