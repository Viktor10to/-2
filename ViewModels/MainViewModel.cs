using Flexi2.Core.MVVM;
using Flexi2.ViewModels.Orders;

namespace Flexi2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public object CurrentView { get; }

        public MainViewModel(NavigationService nav)
        {
            nav.Navigate(new LoginViewModel(nav, session));
        }

    }
}
