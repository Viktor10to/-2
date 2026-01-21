using System;
using System.IO;
using Flexi2.Core.MVVM;
using Flexi2.Navigation;
using Flexi2.Core.Session;
using Flexi2.Data;

namespace Flexi2.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public NavigationService Nav { get; }
        public UserSession Session { get; }

        // repos (ако ги имаш)
        public UserRepository UsersRepo { get; }
        public FloorRepository FloorRepo { get; }
        public MenuRepository MenuRepo { get; }
        public OrderRepository OrderRepo { get; }
        public ReportsRepository ReportsRepo { get; }
        public AuditRepository AuditRepo { get; }

        public MainViewModel()
        {
            // DB path: %AppData%/Flexi2/flexi.db
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath = Path.Combine(appData, "Flexi2", "flexi.db");
            var db = new FlexiDb(dbPath);

            DbInit.EnsureCreated(db);

            UsersRepo = new UserRepository(db);
            FloorRepo = new FloorRepository(db);
            MenuRepo = new MenuRepository(db);
            OrderRepo = new OrderRepository(db);
            ReportsRepo = new ReportsRepository(db);
            AuditRepo = new AuditRepository(db);

            Session = new UserSession();

            Nav = new NavigationService(vm => CurrentViewModel = vm);

            // START SCREEN:
            Nav.NavigateTo(new LoginViewModel(this));
        }
    }
}
