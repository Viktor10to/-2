using Flexi2.Core.MVVM;
using Flexi2.Models;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public sealed class ProductViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        private readonly TicketViewModel _ticket;

        public int TableId { get; }
        public Category Category { get; }

        public ObservableCollection<Product> Products { get; } = new();

        public RelayCommand BackToCategoriesCommand { get; }
        public RelayCommand AddProductCommand { get; }
        public RelayCommand BackToTicketCommand { get; }

        public ProductViewModel(MainViewModel main, int tableId, Category category, TicketViewModel ticket)
        {
            _main = main;
            TableId = tableId;
            Category = category;
            _ticket = ticket;

            BackToCategoriesCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main, tableId, _ticket)));
            BackToTicketCommand = new RelayCommand(_ => _main.Nav.NavigateTo(_ticket));

            AddProductCommand = new RelayCommand(p =>
            {
                if (p is not Product pr) return;
                _ticket.AddDraftProduct(pr);
            });

            Load();
        }

        private void Load()
        {
            Products.Clear();
            foreach (var p in _main.MenuRepo.GetProducts(Category.Id))
                Products.Add(p);
        }
    }
}
