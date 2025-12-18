using FlexiPOS.Core;

namespace FlexiPOS.Services
{
    public sealed class NavigationService : ObservableObject
    {
        private ObservableObject? _currentViewModel;
        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            private set => Set(ref _currentViewModel, value);
        }

        public void NavigateTo(ObservableObject viewModel) => CurrentViewModel = viewModel;
    }
}
