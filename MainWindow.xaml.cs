using System.Windows;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.ViewModels;

namespace Flexi2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Core services
            var navigationService = new NavigationService();
            var session = new UserSession();

            // Main VM
            DataContext = new MainViewModel(navigationService, session);
        }
    }
}
