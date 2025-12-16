using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;

namespace Flexi2.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        public string Title => $"ADMIN PANEL - {_session.DisplayName}";

        public RelayCommand LogoutCommand { get; }

        public AdminViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            LogoutCommand = new RelayCommand(() =>
            {
                _session.Logout();
                _nav.Navigate(new LoginViewModel(_nav, _session));
            });
        }
    }
}
