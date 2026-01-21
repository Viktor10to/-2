using Flexi2.Core.MVVM;
using Flexi2.Models;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public sealed class CategoryViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        private readonly TicketViewModel _ticket;
        public int TableId { get; }

        public ObservableCollection<Category> Categories { get; } = new();

        public RelayCommand BackCommand { get; }
        public RelayCommand OpenCategoryCommand { get; }

        public CategoryViewModel(MainViewModel main, int tableId, TicketViewModel ticket)
        {
            _main = main;
            _ticket = ticket;
            TableId = tableId;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(_ticket));
            OpenCategoryCommand = new RelayCommand(c =>
            {
                if (c is not Category cat) return;
                _main.Nav.NavigateTo(new ProductViewModel(_main, tableId, cat, _ticket));
            });

            Load();
        }

        private void Load()
        {
            Categories.Clear();
            foreach (var c in _main.MenuRepo.GetCategories())
                Categories.Add(c);
        }
    }
}
