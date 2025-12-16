using System.Windows;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.ViewModels;

namespace Flexi2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var session = new UserSession();
            var nav = new NavigationService();

            var mainVm = new MainViewModel(nav, session);
            var window = new MainWindow
            {
                DataContext = mainVm
            };

            nav.Navigate(new LoginViewModel(nav, session));

            window.Show();
        }
    }
}
