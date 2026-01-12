using System.Collections.ObjectModel;
using System.Linq;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class TicketViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        public int TableId { get; }

        private int _orderId;
        public int OrderId { get => _orderId; private set { _orderId = value; OnPropertyChanged(); } }

        public ObservableCollection<OrderItem> LockedItems { get; } = new();
        public ObservableCollection<DraftLine> DraftItems { get; } = new();

        private decimal _discountPercent;
        public decimal DiscountPercent { get => _discountPercent; set { _discountPercent = value; OnPropertyChanged(); Recalc(); } }

        private decimal _total;
        public decimal Total { get => _total; private set { _total = value; OnPropertyChanged(); } }

        private decimal _totalAfterDiscount;
        public decimal TotalAfterDiscount { get => _totalAfterDiscount; private set { _totalAfterDiscount = value; OnPropertyChanged(); } }

        public RelayCommand BackCommand { get; }
        public RelayCommand SubmitCommand { get; }
        public RelayCommand CloseBillCommand { get; }
        public RelayCommand RemoveDraftLineCommand { get; }
        public RelayCommand IncDraftQtyCommand { get; }
        public RelayCommand DecDraftQtyCommand { get; }
        public RelayCommand OpenMenuCommand { get; }

        public TicketViewModel(MainViewModel main, int tableId)
        {
            _main = main;
            TableId = tableId;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new FloorPlanViewModel(_main)));
            OpenMenuCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main))); // избор на продукт ще добавим през shared DraftService следващ пакет
            SubmitCommand = new RelayCommand(_ => SubmitDraft());
            CloseBillCommand = new RelayCommand(_ => CloseBill());
            RemoveDraftLineCommand = new RelayCommand(x => RemoveDraft((DraftLine)x!));
            IncDraftQtyCommand = new RelayCommand(x => { var d = (DraftLine)x!; d.Qty++; Recalc(); });
            DecDraftQtyCommand = new RelayCommand(x => { var d = (DraftLine)x!; if (d.Qty > 1) { d.Qty--; Recalc(); } });

            EnsureOrder();
            LoadLocked();
            Recalc();
        }

        private void EnsureOrder()
        {
            var openId = _main.OrderRepo.GetOpenOrderIdForTable(TableId);
            var me = _main.Session.CurrentUser!;
            if (openId == null)
            {
                OrderId = _main.OrderRepo.OpenOrder(TableId, me.Id);
                _main.FloorRepo.SetTableOccupied(TableId, me.Id, System.DateTime.UtcNow);
                _main.AuditRepo.Log(me.Id, "OPEN_TABLE", "Table", TableId, $"Opened order #{OrderId}");
            }
            else
            {
                OrderId = openId.Value;
            }
        }

        private void LoadLocked()
        {
            LockedItems.Clear();
            foreach (var i in _main.OrderRepo.GetItems(OrderId))
                LockedItems.Add(i);

            // total to table
            var tot = _main.OrderRepo.CalcTotal(OrderId);
            _main.FloorRepo.UpdateTableTotal(TableId, tot);
        }

        // В този MVP добавяме draft ръчно от UI (бутон тест) – после го връзваме от ProductView
        public void AddDraftProduct(Product p)
        {
            var existing = DraftItems.FirstOrDefault(x => x.ProductId == p.Id);
            if (existing != null) { existing.Qty++; Recalc(); return; }
            DraftItems.Add(new DraftLine(p.Id, p.Name, p.Price, 1));
            Recalc();
        }

        private void SubmitDraft()
        {
            if (DraftItems.Count == 0) return;

            var items = DraftItems.Select(d => (p: new Product { Id = d.ProductId, Name = d.Name, Price = d.Price }, qty: d.Qty)).ToList();
            _main.OrderRepo.InsertLockedItems(OrderId, items);

            var me = _main.Session.CurrentUser!;
            _main.AuditRepo.Log(me.Id, "SUBMIT", "Order", OrderId, $"Submitted {DraftItems.Count} lines");

            DraftItems.Clear();
            LoadLocked();
            Recalc();
        }

        private void CloseBill()
        {
            var me = _main.Session.CurrentUser!;
            // POS може само своя маса
            // (в този MVP приемаме че ако е стигнал тук е ok; можеш да добавиш check от Tables table)

            var total = _main.OrderRepo.CalcTotal(OrderId);
            var after = total * (1m - (DiscountPercent / 100m));
            if (after < 0) after = 0;

            _main.OrderRepo.CloseOrder(OrderId, DiscountPercent, "Cash");
            _main.OrderRepo.InsertTurnover(me.Id, TableId, after);
            _main.FloorRepo.SetTableFree(TableId);
            _main.AuditRepo.Log(me.Id, "CLOSE", "Order", OrderId, $"Closed total={total:0.00}, after={after:0.00}, discount={DiscountPercent:0.##}%");

            _main.Nav.NavigateTo(new FloorPlanViewModel(_main));
        }

        private void RemoveDraft(DraftLine d)
        {
            DraftItems.Remove(d);
            Recalc();
        }

        private void Recalc()
        {
            var locked = LockedItems.Sum(x => x.PriceSnapshot * x.Qty);
            var draft = DraftItems.Sum(x => x.Price * x.Qty);
            Total = locked + draft;

            var after = Total * (1m - (DiscountPercent / 100m));
            TotalAfterDiscount = after < 0 ? 0 : after;
        }

        public sealed class DraftLine : ObservableObject
        {
            public int ProductId { get; }
            public string Name { get; }
            public decimal Price { get; }

            private int _qty;
            public int Qty { get => _qty; set { _qty = value; OnPropertyChanged(); } }

            public DraftLine(int productId, string name, decimal price, int qty)
            {
                ProductId = productId; Name = name; Price = price; _qty = qty;
            }
        }
    }
}
