using System.Windows;
using FlexiPOS.Core;
using FlexiPOS.Services;

namespace FlexiPOS.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        public NavigationService Navigation { get; } = new NavigationService();

        public bool CanCloseApp { get; private set; } = false;

        public RelayCommand ExitCommand { get; }

        public MainViewModel()
        {
            ExitCommand = new RelayCommand(ExitApp);

            // старт -> Login
            Navigation.NavigateTo(new LoginViewModel(Navigation));
        }

        private void ExitApp()
        {
            CanCloseApp = true;
            Application.Current.Shutdown();
        }
    }
}
