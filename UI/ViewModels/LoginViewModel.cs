using System.Windows;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        private string _pin = string.Empty;
        public string Pin
        {
            get => _pin;
            set { _pin = value; OnPropertyChanged(); }
        }

        private string _error = string.Empty;
        public string Error
        {
            get => _error;
            set { _error = value; OnPropertyChanged(); }
        }

        private bool _isAdminSelected;
        public bool IsAdminSelected
        {
            get => _isAdminSelected;
            set { _isAdminSelected = value; OnPropertyChanged(); }
        }

        // UI commands
        public RelayCommand SelectPosCommand { get; }
        public RelayCommand SelectAdminCommand { get; }

        public RelayCommand EnterCommand { get; }
        public RelayCommand ExitCommand { get; }

        public LoginViewModel(MainViewModel main)
        {
            _main = main;

            SelectPosCommand = new RelayCommand(_ => SelectPos());
            SelectAdminCommand = new RelayCommand(_ => SelectAdmin());

            EnterCommand = new RelayCommand(_ => Login());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // default: POS
            IsAdminSelected = false;
        }

        private void SelectPos()
        {
            IsAdminSelected = false;
            Error = string.Empty;
        }

        private void SelectAdmin()
        {
            IsAdminSelected = true;
            Error = string.Empty;
        }

        private void Login()
        {
            Error = string.Empty;
            var pin = (Pin ?? string.Empty).Trim();

            if (pin.Length == 0)
            {
                Error = "Въведи PIN";
                return;
            }

            var role = IsAdminSelected ? UserRole.Admin : UserRole.Pos;

            var user = _main.UsersRepo.TryLoginByPin(pin, role);
            if (user == null)
            {
                Error = IsAdminSelected ? "Грешен admin код" : "Този код не е POS";
                return;
            }

            _main.Session.SetUser(user);

            if (user.Role == UserRole.Admin)
                _main.Nav.NavigateTo(new AdminViewModel(_main));
            else
                _main.Nav.NavigateTo(new FloorPlanViewModel(_main));
        }
    }
}
