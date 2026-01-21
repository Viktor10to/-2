using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flexi2.Core.MVVM
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Това ти оправя грешката "The name 'Set' does not exist..."
        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }

        // Alias за по-лесен copy/paste между ViewModel-и
        // (някои класове ползват SetProperty вместо Set)
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
            => Set(ref field, value, name);
    }
}
