using Flexi2.Core.MVVM;
using Flexi2.ViewModels;

namespace Flexi2.Core.Session
{
    public enum UserRole
    {
        Waiter,
        Admin
    }

    public class UserSession : ObservableObject
    {
        public decimal TotalTurnover { get; set; }
        public FloorPlanViewModel? FloorPlan { get; set; }
        private bool _isLoggedIn;
        private string _displayName = "";
        private UserRole _role = UserRole.Waiter;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { _isLoggedIn = value; OnPropertyChanged(); }
        }

        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        public UserRole Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public void Logout()
        {
            IsLoggedIn = false;
            DisplayName = "";
            Role = UserRole.Waiter;
        }
    }
}
