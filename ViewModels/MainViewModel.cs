using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;

namespace Flexi2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        private object? _currentView;

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            _nav = new NavigationService();
            _nav.Changed += () => CurrentView = _nav.Current;

            _session = new UserSession();

            // старт → Login
            _nav.Navigate(new LoginViewModel(_nav, _session));
        }
    }
}
