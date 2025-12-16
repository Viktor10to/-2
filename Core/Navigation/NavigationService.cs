using Flexi2.Core.MVVM;

namespace Flexi2.Core.Navigation
{
    public class NavigationService : ObservableObject
    {
        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public void Navigate(object viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
