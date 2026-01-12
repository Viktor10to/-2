using Flexi2.Core.MVVM;

namespace Flexi2.ViewModels
{
    public sealed class AdminViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public string Title => $"ADMIN • {_main.Session.CurrentUser?.Name}";

        private object? _current;
        public object? Current { get => _current; set { _current = value; OnPropertyChanged(); } }

        public RelayCommand OpenUsersCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand OpenFloorCommand { get; }


        public AdminViewModel(MainViewModel main)
        {
            _main = main;
            OpenFloorCommand = new RelayCommand(_ => Current = new AdminFloorViewModel(_main));
            OpenUsersCommand = new RelayCommand(_ => Current = new AdminUsersViewModel(_main));
            LogoutCommand = new RelayCommand(_ => _main.LogoutCommand.Execute(null));

            Current = new AdminUsersViewModel(_main);
        }
    }
}
