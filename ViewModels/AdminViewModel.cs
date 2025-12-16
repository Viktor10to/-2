using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Data;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;
        private readonly AdminRepository _repo;

        public decimal TotalRevenue { get; }
        public int OrdersCount { get; }
        public ObservableCollection<OrderRow> LastOrders { get; }

        public RelayCommand LogoutCommand { get; }

        public AdminViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            var db = new FlexiDb();
            _repo = new AdminRepository(db);

            TotalRevenue = _repo.GetTotalRevenue();
            OrdersCount = _repo.GetOrdersCount();
            LastOrders = new ObservableCollection<OrderRow>(_repo.GetLastOrders());

            LogoutCommand = new RelayCommand(() =>
            {
                _session.Logout();
                _nav.Navigate(new LoginViewModel(_nav, _session));
            });
        }
    }
}
