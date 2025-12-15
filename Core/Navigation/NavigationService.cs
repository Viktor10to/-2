using System;

namespace Flexi2.Core.Navigation
{
    public class NavigationService
    {
        public object? Current { get; private set; }
        public event Action? Changed;

        public void Navigate(object vm)
        {
            Current = vm;
            Changed?.Invoke();
        }
    }
}
