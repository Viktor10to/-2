using Flexi2.Core.MVVM;
using Flexi2.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Flexi2.ViewModels;

namespace Flexi2.ViewModels
{
    public sealed class TicketViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        public int TableId { get; }

        public ObservableCollection<OrderItem> LockedItems { get; } = new();
        public ObservableCollection<OrderItem> DraftItems { get; } = new();

        private string _title = "";
        public string Title { get => _title; set => Set(ref _title, value); }

        private decimal _total;
        public decimal Total { get => _total; set => Set(ref _total, value); }

        private string _error = "";
        public string Error { get => _error; set => Set(ref _error, value); }

        public RelayCommand BackToFloorCommand { get; }
        public RelayCommand AddItemsCommand { get; }
        public RelayCommand IncDraftCommand { get; }
        public RelayCommand DecDraftCommand { get; }
        public RelayCommand PlaceOrderCommand { get; }
        public RelayCommand CloseBillCommand { get; }

        public TicketViewModel(MainViewModel main, int tableId)
        {
            _main = main;
            TableId = tableId;

            Error = "";

            BackToFloorCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new FloorPlanViewModel(_main)));
            AddItemsCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main, tableId, this)));

            IncDraftCommand = new RelayCommand(o =>
            {
                if (o is not OrderItem it) return;
                it.Qty++;
                Recalc();
            });

            DecDraftCommand = new RelayCommand(o =>
            {
                if (o is not OrderItem it) return;
                if (it.Qty > 1) it.Qty--;
                else DraftItems.Remove(it);
                Recalc();
            });

            PlaceOrderCommand = new RelayCommand(_ => PlaceOrder(), _ => DraftItems.Count > 0);
            // Opens payment screen (final close happens there)
            CloseBillCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new PaymentViewModel(_main, this)));

            Load();
        }

        public void AddDraftProduct(Product p)
        {
            var existing = DraftItems.FirstOrDefault(x => x.ProductId == p.Id && x.ProductName == p.Name);
            if (existing != null) existing.Qty++;
            else
                DraftItems.Add(new OrderItem
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitPrice = p.Price,
                    Qty = 1,
                    IsLocked = false
                });

            Recalc();
            PlaceOrderCommand.RaiseCanExecuteChanged();
        }

        private void Load()
        {
            Error = "";
            LockedItems.Clear();
            foreach (var it in _main.OrderRepo.GetLockedItems(TableId))
                LockedItems.Add(it);

            Title = $"Сметка • Маса #{TableId}";
            Recalc();
        }

        private void Recalc()
        {
            var locked = LockedItems.Sum(i => i.UnitPrice * i.Qty);
            var draft = DraftItems.Sum(i => i.UnitPrice * i.Qty);
            Total = locked + draft;
        }

        private void PlaceOrder()
        {
            Error = "";
            var user = _main.Session.CurrentUser;
            if (user == null) return;

            // ако масата е free → правим occupied
            _main.FloorRepo.SetTableOccupied(TableId, user.Id);

            var orderId = _main.OrderRepo.EnsureOpenOrder(TableId, user.Id);

            _main.OrderRepo.AddLockedItems(orderId, DraftItems.Select(d => new OrderItem
            {
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                UnitPrice = d.UnitPrice,
                Qty = d.Qty,
                IsLocked = true
            }));

            DraftItems.Clear();

            // reload locked
            LockedItems.Clear();
            foreach (var it in _main.OrderRepo.GetLockedItems(TableId))
                LockedItems.Add(it);

            var total = _main.OrderRepo.GetCurrentTotal(TableId);
            _main.FloorRepo.UpdateTableTotal(TableId, total);

            Recalc();
            PlaceOrderCommand.RaiseCanExecuteChanged();
        }

        // Closing is handled in PaymentViewModel
    }
}
