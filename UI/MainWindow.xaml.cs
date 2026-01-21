using System.Windows;
using Flexi2.ViewModels;

namespace Flexi2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // App-wide session (if you use it) lives in Core.Session/UserSession.
            // MainViewModel creates/owns a UserSession instance too.
            DataContext = new MainViewModel();
        }
    }
}
