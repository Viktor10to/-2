using Flexi2.Core.MVVM;

namespace Flexi2.Models
{
    public class OrderItem : ObservableObject
    {
        private int _qty = 1;
        private bool _isLocked;

        public Product Product { get; set; } = null!;

        public int Qty
        {
            get => _qty;
            set
            {
                if (IsLocked) return;

                _qty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged();
            }
        }

        public decimal Total => Product.Price * Qty;
    }
}
