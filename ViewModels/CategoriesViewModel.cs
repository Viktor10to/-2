using System.Collections.ObjectModel;
using FlexiPOS.Core;
using FlexiPOS.Models;
using FlexiPOS.Services;

namespace FlexiPOS.ViewModels
{
    public sealed class CategoriesViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly SessionContext _session;
        private readonly TableModel _table;

        // shared store (в реалния проект ще е DI + SQLite)
        private static readonly InMemoryDataStore Store = new InMemoryDataStore();

        public CategoriesViewModel(NavigationService nav, SessionContext session, TableModel table)
        {
            _nav = nav;
            _session = session;
            _table = table;

            Title = $"Маса {_table.Name} • Категории";

            Categories = new ObservableCollection<CategoryModel>(Store.Categories);

            BackCommand = new RelayCommand(BackToFloor);
            OpenCategoryCommand = new RelayCommand(c => OpenCategory((CategoryModel)c!));
        }

        public string Title { get; }
        public ObservableCollection<CategoryModel> Categories { get; }

        public RelayCommand BackCommand { get; }
        public RelayCommand OpenCategoryCommand { get; }

        private void BackToFloor() => _nav.NavigateTo(new FloorPlanViewModel(_nav, _session));

        private void OpenCategory(CategoryModel cat)
            => _nav.NavigateTo(new ProductsViewModel(_nav, _session, _table, cat, Store));
    }
}
