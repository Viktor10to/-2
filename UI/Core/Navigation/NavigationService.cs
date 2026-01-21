using System;

namespace Flexi2.Navigation
{
    public sealed class NavigationService
    {
        private readonly Action<object> _setVm;

        public NavigationService(Action<object> setVm)
        {
            _setVm = setVm;
        }

        public void NavigateTo(object vm)
        {
            _setVm(vm);
        }
    }
}
