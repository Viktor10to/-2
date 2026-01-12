using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class ProductViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        public Category Category { get; }

        public ObservableCollection<Product> Products { get; } = new();

        public RelayCommand BackCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public ProductViewModel(MainViewModel main, Category category)
        {
            _main = main;
            Category = category;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main)));
            LogoutCommand = new RelayCommand(_ => _main.LogoutCommand.Execute(null));

            Load();
        }

        private void Load()
        {
            Products.Clear();
            foreach (var p in _main.AdminRepo.GetProductsByCategory(Category.Id))
                Products.Add(p);
        }
    }
}
