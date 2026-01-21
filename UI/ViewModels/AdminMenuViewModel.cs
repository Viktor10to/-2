using System;
using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class AdminMenuViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                LoadProducts();
            }
        }

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private string _newCategoryName = "";
        public string NewCategoryName
        {
            get => _newCategoryName;
            set => SetProperty(ref _newCategoryName, value);
        }

        private string _newProductName = "";
        public string NewProductName
        {
            get => _newProductName;
            set => SetProperty(ref _newProductName, value);
        }

        // keep as text for easy binding ("2.50" or "2,50")
        private string _newProductPrice = "";
        public string NewProductPrice
        {
            get => _newProductPrice;
            set => SetProperty(ref _newProductPrice, value);
        }

        private string _error = "";
        public string Error
        {
            get => _error;
            set { _error = value; OnPropertyChanged(); }
        }

        public RelayCommand BackCommand { get; }
        public RelayCommand BackToActivitiesCommand { get; }
        public RelayCommand AddCategoryCommand { get; }
        public RelayCommand DeleteCategoryCommand { get; }
        public RelayCommand ToggleCategoryActiveCommand { get; }
        public RelayCommand AddProductCommand { get; }
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand ToggleProductActiveCommand { get; }

        public AdminMenuViewModel(MainViewModel main)
        {
            _main = main;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminViewModel(_main)));
            BackToActivitiesCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });

            AddCategoryCommand = new RelayCommand(_ =>
            {
                try
                {
                    Error = "";
                    var name = (NewCategoryName ?? "").Trim();
                    if (name.Length < 2) { Error = "Името на категорията е твърде кратко."; return; }

                    _main.MenuRepo.AddCategory(name);
                    NewCategoryName = "";
                    LoadCategories(selectLast: true);
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            ToggleCategoryActiveCommand = new RelayCommand(_ =>
            {
                try
                {
                    Error = "";
                    if (SelectedCategory is null) { Error = "Избери категория."; return; }
                    _main.MenuRepo.SetCategoryActive(SelectedCategory.Id, !SelectedCategory.IsActive);
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            DeleteCategoryCommand = new RelayCommand(_ =>
            {
                try
                {
                    Error = "";
                    if (SelectedCategory is null) { Error = "Избери категория."; return; }
                    _main.MenuRepo.DeleteCategory(SelectedCategory.Id);
                    SelectedCategory = null;
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            AddProductCommand = new RelayCommand(_ =>
            {
                try
                {
                    Error = "";
                    if (SelectedCategory is null) { Error = "Първо избери категория."; return; }

                    var name = (NewProductName ?? "").Trim();
                    if (name.Length < 2) { Error = "Името на артикула е твърде кратко."; return; }

                    if (!decimal.TryParse((NewProductPrice ?? "").Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
                    {
                        Error = "Невалидна цена.";
                        return;
                    }

                    _main.MenuRepo.AddProduct(SelectedCategory.Id, name, price, hasModifiers: false);
                    NewProductName = "";
                    NewProductPrice = "";
                    LoadProducts(selectLast: true);
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            DeleteProductCommand = new RelayCommand(p =>
            {
                try
                {
                    Error = "";
                    var pr = p as Product ?? SelectedProduct;
                    if (pr is null) { Error = "Избери артикул."; return; }
                    _main.MenuRepo.DeleteProduct(pr.Id);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            ToggleProductActiveCommand = new RelayCommand(p =>
            {
                try
                {
                    Error = "";
                    var pr = p as Product ?? SelectedProduct;
                    if (pr is null) return;
                    _main.MenuRepo.SetProductActive(pr.Id, !pr.IsActive);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            });

            LoadCategories();
        }

        private void LoadCategories(bool selectLast = false)
        {
            Categories.Clear();
            foreach (var c in _main.MenuRepo.GetCategories(includeInactive: true))
                Categories.Add(c);

            if (Categories.Count == 0)
            {
                SelectedCategory = null;
                Products.Clear();
                return;
            }

            if (selectLast)
                SelectedCategory = Categories[^1];
            else if (SelectedCategory is null)
                SelectedCategory = Categories[0];
            else
            {
                // keep selection if still exists
                for (int i = 0; i < Categories.Count; i++)
                    if (Categories[i].Id == SelectedCategory.Id)
                    {
                        SelectedCategory = Categories[i];
                        break;
                    }
            }
        }

        private void LoadProducts(bool selectLast = false)
        {
            Products.Clear();
            if (SelectedCategory is null) return;

            foreach (var p in _main.MenuRepo.GetProducts(SelectedCategory.Id, includeInactive: true))
                Products.Add(p);

            if (selectLast && Products.Count > 0)
                SelectedProduct = Products[^1];
        }
    }
}
