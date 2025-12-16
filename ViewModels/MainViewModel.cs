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

        public MainViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            _nav.PropertyChanged += (_, __) =>
                OnPropertyChanged(nameof(CurrentView));

            // създаваме FloorPlan САМО ВЕДНЪЖ
            _session.FloorPlan = new FloorPlanViewModel(_nav, _session);

            _nav.Navigate(new LoginViewModel(_nav, _session));
        }
    }

}
