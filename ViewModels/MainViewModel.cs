using Flexi2.Core.MVVM;
using Flexi2.Data;
using Flexi2.Navigation;
using Flexi2.Session;

namespace Flexi2.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        public NavigationService Nav { get; }
        public UserSession Session { get; }

        public FlexiDb Db { get; }
        public UserRepository UsersRepo { get; }
        public AdminRepository AdminRepo { get; }

        public RelayCommand LogoutCommand { get; }
        public FloorRepository FloorRepo { get; }
        public AuditRepository AuditRepo { get; }
        public OrderRepository OrderRepo { get; }


        public MainViewModel()
        {
            FloorRepo = new FloorRepository(Db);
            AuditRepo = new AuditRepository(Db);
            OrderRepo = new OrderRepository(Db);

            Nav = new NavigationService();
            Session = new UserSession();

            Db = new FlexiDb("flexi.db");
            DbInit.EnsureCreated(Db);

            UsersRepo = new UserRepository(Db);
            AdminRepo = new AdminRepository(Db);

            LogoutCommand = new RelayCommand(_ =>
            {
                Session.Clear();
                Nav.NavigateTo(new LoginViewModel(this));
            });

            Nav.NavigateTo(new LoginViewModel(this));
        }
    }
}
