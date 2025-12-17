using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Data;
using Flexi2.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flexi2.ViewModels
{
    public class OrderViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;
        private readonly TableModel _table;

        // ================== DATA ==================
        public ObservableCollection<Category> Categories { get; }
            = new ObservableCollection<Category>();

        public ObservableCollection<Product> Products { get; }
            = new ObservableCollection<Product>();

        public ObservableCollection<Product> FilteredProducts { get; }
            = new ObservableCollection<Product>();

        public ObservableCollection<OrderItem> OrderItems => _table.OrderItems;

        private Category? _selectedCategory;

        // ================== DISCOUNT ==================
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                _discountPercent = value;
                OnPropertyChanged();
                OnTotalsChanged();
            }
        }

        // ================== TOTALS ==================
        public decimal Subtotal => OrderItems.Sum(i => i.Total);
        public decimal DiscountAmount => Subtotal * DiscountPercent / 100m;
        public decimal FinalTotal => Subtotal - DiscountAmount;

        public string Title => $"МАСА {_table.Number}";

        // ================== COMMANDS ==================
        public RelayCommand<Category> SelectCategoryCommand { get; }
        public RelayCommand<Product> AddProductCommand { get; }
        public RelayCommand SubmitOrderCommand { get; }
        public RelayCommand CloseBillCommand { get; }
        public RelayCommand BackCommand { get; }

        // ================== CTOR ==================
        public OrderViewModel(
            NavigationService nav,
            UserSession session,
            TableModel table)
        {
            _nav = nav;
            _session = session;
            _table = table;
        

            // -------- TEMP DATA (по-късно от DB) --------
            Categories.Add(new Category { Name = "Кафе" });
            Categories.Add(new Category { Name = "Безалкохолни" });
            Categories.Add(new Category { Name = "Храна" });

            Products.Add(new Product { Name = "Еспресо", Price = 2.50m, Category = "Кафе" });
            Products.Add(new Product { Name = "Кола", Price = 3.00m, Category = "Безалкохолни" });
            Products.Add(new Product { Name = "Бургер", Price = 8.00m, Category = "Храна" });

            // ================== COMMAND LOGIC ==================

            SelectCategoryCommand = new RelayCommand<Category>(cat =>
            {
                _selectedCategory = cat;
                FilteredProducts.Clear();

                foreach (var p in Products.Where(p => p.Category == cat.Name))
                    FilteredProducts.Add(p);
            });

            AddProductCommand = new RelayCommand<Product>(p =>
            {
                var existing = OrderItems.FirstOrDefault(i => i.Product == p);

                if (existing != null)
                    existing.Qty++;
                else
                    OrderItems.Add(new OrderItem { Product = p });

                _table.Status = TableStatus.Busy;
                _table.HasOpenOrder = true;

                OnTotalsChanged();
            });

            SubmitOrderCommand = new RelayCommand(() =>
            {
                _table.Status = TableStatus.Busy;
                _table.HasOpenOrder = true;

                foreach (var item in OrderItems)
                    item.IsLocked = true;

                _nav.Navigate(_session.FloorPlan!);
            });


            CloseBillCommand = new RelayCommand(() =>
            {
                // Запис в оборота
                _session.TurnoverHistory.Add(new TurnoverEntry
                {
                    Time = DateTime.Now,
                    Amount = FinalTotal
                });

                _session.TotalTurnover += FinalTotal;

                // Чистим масата
                OrderItems.Clear();
                _table.HasOpenOrder = false;
                _table.Status = TableStatus.Free;

                DiscountPercent = 0;

                _nav.Navigate(_session.FloorPlan!);

            });

            BackCommand = new RelayCommand(() =>
            {
                _nav.Navigate(_session.FloorPlan!);
            });
        }

        // ================== HELPERS ==================
        private void OnTotalsChanged()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(FinalTotal));
        }
    }
}
