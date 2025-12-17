using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;

namespace Flexi2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        public object CurrentView => _nav.CurrentViewModel;

        public RelayCommand LogoutCommand { get; }

        public MainViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            _nav.PropertyChanged += (_, __) =>
                OnPropertyChanged(nameof(CurrentView));

            LogoutCommand = new RelayCommand(Logout);

            // старт винаги от Login
            _nav.Navigate(new LoginViewModel(_nav, _session));
        }

        private void Logout()
        {
            _session.Clear();
            _nav.Navigate(new LoginViewModel(_nav, _session));
        }

    }
}
