using System.Collections.ObjectModel;
using System.Linq;
using FlexiPOS.Core;
using FlexiPOS.Models;
using FlexiPOS.Services;

namespace FlexiPOS.ViewModels
{
    public sealed class ProductsViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly SessionContext _session;
        private readonly TableModel _table;
        private readonly CategoryModel _category;
        private readonly InMemoryDataStore _store;

        private readonly TableOrderState _orderState;

        public ProductsViewModel(NavigationService nav, SessionContext session, TableModel table, CategoryModel category, InMemoryDataStore store)
        {
            _nav = nav;
            _session = session;
            _table = table;
            _category = category;
            _store = store;

            Title = $"Маса {_table.Name} • {_category.Name}";

            // при POS: собственикът е сервитьорът, при Admin може да отваря всяка
            var ownerId = _table.OwnerUserId ?? _session.UserId;
            _orderState = _store.GetOrCreateOrder(_table.Id, ownerId);

            Products = new ObservableCollection<ProductModel>(
                _store.Products.Where(p => p.CategoryId == _category.Id));

            Lines = new ObservableCollection<OrderLine>(_orderState.Lines);

            AddProductCommand = new RelayCommand(p => AddProduct((ProductModel)p!));
            IncDraftCommand = new RelayCommand(l => IncDraft((OrderLine)l!), l => CanEditDraft((OrderLine)l!));
            DecDraftCommand = new RelayCommand(l => DecDraft((OrderLine)l!), l => CanEditDraft((OrderLine)l!));

            OrderCommand = new RelayCommand(PlaceOrder, CanPlaceOrder);
            BackCommand = new RelayCommand(() => _nav.NavigateTo(new CategoriesViewModel(_nav, _session, _table)));
            FinishCommand = new RelayCommand(FinishBill, CanFinish);

            RefreshTableMeta();
        }

        public string Title { get; }
        public ObservableCollection<ProductModel> Products { get; }
        public ObservableCollection<OrderLine> Lines { get; }

        private string _toast = "";
        public string Toast { get => _toast; set => Set(ref _toast, value); }

        public RelayCommand AddProductCommand { get; }
        public RelayCommand IncDraftCommand { get; }
        public RelayCommand DecDraftCommand { get; }
        public RelayCommand OrderCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand FinishCommand { get; }

        private bool CanEditDraft(OrderLine l) => !l.IsLocked;

        private void AddProduct(ProductModel p)
        {
            Toast = "";

            // ако има модификатори -> по-късно отваряш ModifierView (UserControl)
            // сега добавяме директно
            var existingDraft = _orderState.Lines.FirstOrDefault(x => !x.IsLocked && x.ProductId == p.Id && x.NoteOrModifiers == null);
            if (existingDraft != null)
            {
                existingDraft.Qty++;
            }
            else
            {
                _orderState.Lines.Add(new OrderLine
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitPrice = p.Price,
                    Qty = 1,
                    IsLocked = false
                });
            }

            SyncLines();
            OrderCommand.RaiseCanExecuteChanged();
        }

        private void IncDraft(OrderLine l)
        {
            l.Qty++;
            SyncLines();
        }

        private void DecDraft(OrderLine l)
        {
            l.Qty--;
            if (l.Qty <= 0)
                _orderState.Lines.Remove(l);

            SyncLines();
            OrderCommand.RaiseCanExecuteChanged();
        }

        private bool CanPlaceOrder()
            => _orderState.Lines.Any(x => !x.IsLocked);

        private void PlaceOrder()
        {
            // “Поръчай” -> lock
            _store.LockDraftLines(_table.Id);

            // масата става заета
            _table.Status = TableStatus.Occupied;
            _table.OwnerUserId ??= _orderState.OwnerUserId;
            _table.OpenedAt ??= _orderState.OpenedAt;

            RefreshTableMeta();
            SyncLines();

            Toast = "Поръчката е записана (demo store).";
            OrderCommand.RaiseCanExecuteChanged();
        }

        private bool CanFinish()
        {
            // само собственика или админ
            if (_session.Role == UserRole.Admin) return true;
            return _table.OwnerUserId == _session.UserId;
        }

        private void FinishBill()
        {
            if (!CanFinish())
            {
                Toast = "Нямаш право да приключиш тази маса.";
                return;
            }

            // close
            _store.CloseOrder(_table.Id);

            _table.Status = TableStatus.Free;
            _table.OwnerUserId = null;
            _table.OpenedAt = null;
            _table.CurrentTotal = 0m;

            Toast = "Сметката е приключена (demo).";
            _nav.NavigateTo(new FloorPlanViewModel(_nav, _session));
        }

        private void RefreshTableMeta()
        {
            _table.CurrentTotal = _store.GetTableTotal(_table.Id);
        }

        private void SyncLines()
        {
            Lines.Clear();
            foreach (var l in _orderState.Lines)
                Lines.Add(l);

            OnPropertyChanged(nameof(Lines));
            IncDraftCommand.RaiseCanExecuteChanged();
            DecDraftCommand.RaiseCanExecuteChanged();

            RefreshTableMeta();
        }
    }
}
