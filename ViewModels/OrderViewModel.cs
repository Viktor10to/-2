using Dapper;
using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Data;
using Flexi2.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flexi2.ViewModels
{
    public class OrderViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;
        private readonly TableModel _table;
        private readonly OrderRepository _repo;

        private int _orderId;

        public string Title => $"МАСА {_table.Number}";

        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<Product> Products { get; }
        public ObservableCollection<OrderItem> Items { get; }

        public decimal Total => Items.Sum(i => i.Total);

        public RelayCommand<Product> AddProductCommand { get; }
        public RelayCommand PlaceOrderCommand { get; }
        public RelayCommand CloseBillCommand { get; }

        public OrderViewModel(NavigationService nav, UserSession session, TableModel table)
        {
            _nav = nav;
            _session = session;
            _table = table;

            var db = new FlexiDb();
            _repo = new OrderRepository(db);

            _orderId = _repo.CreateOrder(_table.Number);
            _table.Status = TableStatus.Busy;

            Categories = new();
            Products = new();
            Items = new();

            // load от DB
            using var cn = db.Open();
            Categories = new ObservableCollection<Category>(cn.Query<Category>("SELECT * FROM Categories"));
            Products = new ObservableCollection<Product>(cn.Query<Product>("SELECT * FROM Products"));

            AddProductCommand = new RelayCommand<Product>(p =>
            {
                var ex = Items.FirstOrDefault(i => i.Product.Id == p.Id);
                if (ex != null) ex.Qty++;
                else Items.Add(new OrderItem { Product = p, Qty = 1 });
                OnPropertyChanged(nameof(Total));
            });

            PlaceOrderCommand = new RelayCommand(() =>
            {
                _repo.AddItems(_orderId, Items);
            });

            CloseBillCommand = new RelayCommand(() =>
            {
                _repo.CloseOrder(_orderId);
                _table.Status = TableStatus.Free;
                _nav.Navigate(new FloorPlanViewModel(_nav, _session));
            });
        }
    }
}
