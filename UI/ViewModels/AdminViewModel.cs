using Flexi2.Core.MVVM;

namespace Flexi2.ViewModels
{
    public sealed class AdminViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public RelayCommand OpenUsersCommand { get; }
        public RelayCommand OpenFloorCommand { get; }
        public RelayCommand OpenMenuCommand { get; }
        public RelayCommand OpenReportsCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public AdminViewModel(MainViewModel main)
        {
            _main = main;

            OpenUsersCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminUsersViewModel(_main)));
            OpenFloorCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminFloorViewModel(_main)));
            OpenMenuCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminMenuViewModel(_main)));
            OpenReportsCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminReportsViewModel(_main)));
            LogoutCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });
        }
    }
}
