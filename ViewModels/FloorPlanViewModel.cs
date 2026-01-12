using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class FloorPlanViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        public string Title => $"POS • {_main.Session.CurrentUser?.Name}";

        public ObservableCollection<TableModel> Tables { get; } = new();

        public RelayCommand OpenMenuCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public FloorPlanViewModel(MainViewModel main)
        {
            _main = main;

            LogoutCommand = new RelayCommand(_ => _main.LogoutCommand.Execute(null));
            OpenMenuCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main)));

            // MVP demo tables (следващия пакет ще ги правим от DB)
            Tables.Add(new TableModel { Id = 1, Name = "T1", Status = TableStatus.Free, CurrentTotal = 0m });
            Tables.Add(new TableModel { Id = 2, Name = "T2", Status = TableStatus.Occupied, CurrentTotal = 25m });
            Tables.Add(new TableModel { Id = 3, Name = "T3", Status = TableStatus.Free, CurrentTotal = 0m });
        }
    }
}
