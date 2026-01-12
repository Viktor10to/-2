using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class CategoryViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<Category> Categories { get; } = new();

        private Category? _selected;
        public Category? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
                if (value != null)
                    _main.Nav.NavigateTo(new ProductViewModel(_main, value));
            }
        }

        public RelayCommand BackCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public CategoryViewModel(MainViewModel main)
        {
            _main = main;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new FloorPlanViewModel(_main)));
            LogoutCommand = new RelayCommand(_ => _main.LogoutCommand.Execute(null));

            Load();
        }

        private void Load()
        {
            Categories.Clear();
            foreach (var c in _main.AdminRepo.GetCategories())
                Categories.Add(c);
        }
    }
}
