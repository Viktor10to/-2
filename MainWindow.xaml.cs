using System.Windows;
using Flexi2.Navigation;
using Flexi2.Session;
using Flexi2.ViewModels;

namespace Flexi2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var nav = new NavigationService();
            var session = new UserSession();

            DataContext = new MainViewModel();
        }
    }
}
