using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flexi2.ViewModels.Orders
{
    public class OrderViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;
        private readonly TableModel _table;
        public RelayCommand CloseBillCommand { get; }

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Product> FilteredProducts { get; } = new();

        // 👇 ВРЪЗКА КЪМ МАСАТА
        public ObservableCollection<OrderItem> OrderItems => _table.OrderItems;

        public decimal Total => OrderItems.Sum(i => i.Total);

        public RelayCommand<Category> SelectCategoryCommand { get; }
        public RelayCommand<Product> AddProductCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand SubmitOrderCommand { get; }
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;

                _discountPercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiscountAmount));
                OnPropertyChanged(nameof(FinalTotal));
            }
        }

        public decimal Subtotal => OrderItems.Sum(i => i.Total);

        public decimal DiscountAmount => Subtotal * (DiscountPercent / 100m);

        public decimal FinalTotal => Subtotal - DiscountAmount;

        public OrderViewModel(NavigationService nav, UserSession session, TableModel table)
        {
            CloseBillCommand = new RelayCommand(() =>
            {
                // 1️⃣ начисляваме оборота
                _session.TotalTurnover += FinalTotal;
                
                // 2️⃣ чистим сметката
                _table.OrderItems.Clear();
                _table.HasOpenOrder = false;

                // 3️⃣ освобождаваме масата
                _table.Status = TableStatus.Free;

                // 4️⃣ обратно към масите
                _nav.Navigate(_session.FloorPlan!);
            });


            _nav = nav;
            _session = session;
            _table = table;

            // demo категории/продукти
            Categories.Add(new Category { Name = "Кафе" });
            Categories.Add(new Category { Name = "Безалкохолни" });
            Categories.Add(new Category { Name = "Храна" });

            Products.Add(new Product { Name = "Еспресо", Price = 2.50m, Category = "Кафе" });
            Products.Add(new Product { Name = "Кола", Price = 3.00m, Category = "Безалкохолни" });
            Products.Add(new Product { Name = "Бургер", Price = 8.00m, Category = "Храна" });

            SelectCategoryCommand = new RelayCommand<Category>(cat =>
            {
                FilteredProducts.Clear();
                foreach (var p in Products.Where(p => p.Category == cat.Name))
                    FilteredProducts.Add(p);
            });

            AddProductCommand = new RelayCommand<Product>(p =>
            {
                var existing = OrderItems.FirstOrDefault(i => i.Product == p);
                if (existing != null) existing.Qty++;
                else OrderItems.Add(new OrderItem { Product = p });

                _table.Status = TableStatus.Busy;
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(DiscountAmount));
                OnPropertyChanged(nameof(FinalTotal));

            });

            BackCommand = new RelayCommand(() =>
            {
                // ⬅ САМО връщаме към масите
                // ❌ НЕ пипаме масата
                // ❌ НЕ заключваме нищо

                _nav.Navigate(_session.FloorPlan!);
            });


            SubmitOrderCommand = new RelayCommand(() =>
            {
                // 🔒 заключваме текущите редове
                foreach (var item in OrderItems)
                    item.IsLocked = true;

                // 🔴 масата става заета
                _table.Status = TableStatus.Busy;
                _table.HasOpenOrder = true;

                // ❌ НЕ чистим OrderItems
                // поръчката остава към масата

                _nav.Navigate(_session.FloorPlan!);
            });




        }
    }
}
