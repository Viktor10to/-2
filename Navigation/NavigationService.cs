using Flexi2.Core.MVVM;

namespace Flexi2.Navigation
{
    public sealed class NavigationService : ObservableObject
    {
        private object? _current;
        public object? Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(); }
        }

        public void NavigateTo(object vm) => Current = vm;
    }
}
