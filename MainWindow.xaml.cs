using System.Windows;
using Flexi2.ViewModels;

namespace Flexi2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
