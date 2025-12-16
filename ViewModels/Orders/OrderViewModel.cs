using Flexi2.Core.MVVM;
using Flexi2.Models;
using Flexi2.Models.Products;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Flexi2.ViewModels.Orders
{
    public class OrderViewModel : ObservableObject
    {
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Product> FilteredProducts { get; } = new();
        public ObservableCollection<OrderItem> OrderItems { get; } = new();

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public ICommand SelectCategoryCommand { get; }
        public ICommand AddProductCommand { get; }

        public decimal Total => OrderItems.Sum(i => i.LineTotal);

        public OrderViewModel()
        {
            // Категории
            Categories.Add(new Category { Id = 1, Name = "Кафе" });
            Categories.Add(new Category { Id = 2, Name = "Безалкохолни" });
            Categories.Add(new Category { Id = 3, Name = "Храна" });

            // Продукти
            Products.Add(new Product { Id = 1, Name = "Еспресо", Price = 2.50m, CategoryId = 1 });
            Products.Add(new Product { Id = 2, Name = "Капучино", Price = 3.50m, CategoryId = 1 });
            Products.Add(new Product { Id = 3, Name = "Кола", Price = 3.00m, CategoryId = 2 });
            Products.Add(new Product { Id = 4, Name = "Фанта", Price = 3.00m, CategoryId = 2 });
            Products.Add(new Product { Id = 5, Name = "Бургер", Price = 8.00m, CategoryId = 3 });

            SelectCategoryCommand = new RelayCommand<Category>(c => SelectedCategory = c);
            AddProductCommand = new RelayCommand<Product>(AddProduct);
        }

        private void FilterProducts()
        {
            FilteredProducts.Clear();
            if (SelectedCategory == null) return;

            foreach (var p in Products.Where(p => p.CategoryId == SelectedCategory.Id))
                FilteredProducts.Add(p);
        }

        private void AddProduct(Product product)
        {
            var existing = OrderItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
                existing.Quantity++;
            else
                OrderItems.Add(new OrderItem { Product = product });

            OnPropertyChanged(nameof(Total));
        }
    }
}
