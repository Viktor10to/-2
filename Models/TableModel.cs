using Flexi2.Core.MVVM;
using System.Collections.ObjectModel;

namespace Flexi2.Models
{
    public class TableModel : ObservableObject
    {
        public double X { get; set; }
        public double Y { get; set; }

        public int Number { get; set; }
        private bool _hasOpenOrder;

        public bool HasOpenOrder
        {
            get => _hasOpenOrder;
            set
            {
                _hasOpenOrder = value;
                OnPropertyChanged();
            }
        }


        private TableStatus _status;
        public TableStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // 👇 ВАЖНО: поръчка по маса
        public ObservableCollection<OrderItem> OrderItems { get; } = new();
    }
}
