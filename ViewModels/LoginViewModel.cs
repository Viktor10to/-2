using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        private string _pin = "";
        public string Pin { get => _pin; set { _pin = value; OnPropertyChanged(); } }

        private string _error = "";
        public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public RelayCommand EnterCommand { get; }
        public RelayCommand ExitCommand { get; }

        public LoginViewModel(MainViewModel main)
        {
            _main = main;
            EnterCommand = new RelayCommand(_ => Login());
            ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        }

        private void Login()
        {
            Error = "";
            if (string.IsNullOrWhiteSpace(Pin))
            {
                Error = "Въведи PIN";
                return;
            }

            var user = _main.UsersRepo.TryLoginByPin(Pin.Trim());
            if (user == null)
            {
                Error = "Грешен код";
                return;
            }

            _main.Session.Set(user);

            if (user.Role == UserRole.Admin)
                _main.Nav.NavigateTo(new AdminViewModel(_main));
            else
                _main.Nav.NavigateTo(new FloorPlanViewModel(_main));
        }
    }
}
