using System.ComponentModel;
using System.Windows;
using FlexiPOS.ViewModels;

namespace FlexiPOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && !vm.CanCloseApp)
            {
                e.Cancel = true; // забраняваме X / Alt+F4
            }
        }
    }
}
