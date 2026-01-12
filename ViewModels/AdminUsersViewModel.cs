using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class AdminUsersViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<User> Users { get; } = new();

        private User? _selected;
        public User? Selected { get => _selected; set { _selected = value; OnPropertyChanged(); } }

        private string _newName = "";
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private string _newPin = "";
        public string NewPin { get => _newPin; set { _newPin = value; OnPropertyChanged(); } }

        private UserRole _newRole = UserRole.Pos;
        public UserRole NewRole { get => _newRole; set { _newRole = value; OnPropertyChanged(); } }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ChangePinCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public AdminUsersViewModel(MainViewModel main)
        {
            _main = main;

            RefreshCommand = new RelayCommand(_ => Load());
            CreateCommand = new RelayCommand(_ => Create());
            SaveCommand = new RelayCommand(_ => SaveSelected());
            ChangePinCommand = new RelayCommand(_ => ChangePin());
            DeleteCommand = new RelayCommand(_ => Delete());

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
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewPin)) return;
            _main.UsersRepo.Create(NewName.Trim(), NewRole, NewPin.Trim());
            NewName = ""; NewPin = ""; NewRole = UserRole.Pos;
            Load();
        }

        private void SaveSelected()
        {
            if (Selected == null) return;
            _main.UsersRepo.UpdateNameRole(Selected.Id, Selected.Name.Trim(), Selected.Role, Selected.IsActive);
            Load();
        }

        private void ChangePin()
        {
            if (Selected == null) return;
            if (string.IsNullOrWhiteSpace(NewPin)) return;
            _main.UsersRepo.ChangePin(Selected.Id, NewPin.Trim());
            NewPin = "";
        }

        private void Delete()
        {
            if (Selected == null) return;
            if (_main.Session.CurrentUser != null && Selected.Id == _main.Session.CurrentUser.Id) return;

            _main.UsersRepo.Delete(Selected.Id);
            Selected = null;
            Load();
        }
    }
}
