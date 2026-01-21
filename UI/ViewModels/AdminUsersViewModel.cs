using Flexi2.Core.MVVM;
using Flexi2.Models;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public sealed class AdminUsersViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<User> Users { get; } = new();

        private User? _selected;
        public User? Selected
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        private string _newName = "";
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private string _newPin = "";
        public string NewPin { get => _newPin; set { _newPin = value; OnPropertyChanged(); } }

        private int _newRoleInt = 0; // 0 POS, 1 ADMIN
        public int NewRoleInt { get => _newRoleInt; set { _newRoleInt = value; OnPropertyChanged(); } }

        private string _error = "";
        public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public RelayCommand CreateCommand { get; }
        public RelayCommand SaveSelectedCommand { get; }
        public RelayCommand ChangePinCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand BackToActivitiesCommand { get; }
        public RelayCommand BackCommand { get; }

        public AdminUsersViewModel(MainViewModel main)
        {
            _main = main;

            CreateCommand = new RelayCommand(_ => Create());
            SaveSelectedCommand = new RelayCommand(_ => SaveSelected());
            ChangePinCommand = new RelayCommand(_ => ChangePin());
            DeleteCommand = new RelayCommand(_ => Delete());
            LogoutCommand = new RelayCommand(_ => { _main.Session.Logout(); _main.Nav.NavigateTo(new LoginViewModel(_main)); });
            BackToActivitiesCommand = new RelayCommand(_ => { _main.Session.Logout(); _main.Nav.NavigateTo(new LoginViewModel(_main)); });
            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminViewModel(_main)));

            Load();
        }

        private void Load()
        {
            Users.Clear();
            foreach (var u in _main.UsersRepo.GetAll())
                Users.Add(u);
        }

        private void Create()
        {
            Error = "";
            var name = (NewName ?? "").Trim();
            var pin = (NewPin ?? "").Trim();

            if (name.Length == 0 || pin.Length == 0)
            {
                Error = "Име и PIN са задължителни";
                return;
            }

            var role = NewRoleInt == 1 ? UserRole.Admin : UserRole.Pos;

            _main.UsersRepo.Create(name, role, pin);

            NewName = "";
            NewPin = "";
            NewRoleInt = 0;
            Load();
        }

        private void SaveSelected()
        {
            Error = "";
            if (Selected == null) { Error = "Избери потребител"; return; }

            _main.UsersRepo.UpdateNameRole(Selected.Id, Selected.Name, Selected.Role, Selected.IsActive);
            Load();
        }

        private void ChangePin()
        {
            Error = "";
            if (Selected == null) { Error = "Избери потребител"; return; }
            var pin = (NewPin ?? "").Trim();
            if (pin.Length == 0) { Error = "Въведи нов PIN в полето PIN"; return; }

            _main.UsersRepo.ChangePin(Selected.Id, pin);
            NewPin = "";
            Load();
        }

        private void Delete()
        {
            Error = "";
            if (Selected == null) { Error = "Избери потребител"; return; }
            _main.UsersRepo.Delete(Selected.Id);
            Selected = null;
            Load();
        }
    }
}
