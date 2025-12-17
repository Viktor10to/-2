using Flexi2.Core.MVVM;
using Flexi2.ViewModels;
using System.Collections.ObjectModel;
using Flexi2.Models;


namespace Flexi2.Core.Session
{
    public enum UserRole
    {
        Waiter,
        Admin,
        None
    }

    public class UserSession : ObservableObject
    {
        public decimal TotalTurnover { get; set; }
        public FloorPlanViewModel? FloorPlan { get; set; }
        private bool _isLoggedIn;
        private string _displayName = "";
        private UserRole _role = UserRole.Waiter;
        public ObservableCollection<TurnoverEntry> TurnoverHistory { get; }
    = new ObservableCollection<TurnoverEntry>();
        public void Clear()
        {
            IsLoggedIn = false;
            Role = UserRole.None;
            DisplayName = string.Empty;
        }

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
