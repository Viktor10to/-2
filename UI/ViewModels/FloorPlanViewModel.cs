using System;
using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class FloorPlanViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public string Title => "POS • FLOOR PLAN";

        public ObservableCollection<ZoneModel> Zones { get; } = new();
        public ObservableCollection<TableModel> Tables { get; } = new();

        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set
            {
                if (Set(ref _selectedZone, value))
                    ReloadTables();
            }
        }

        private string _error = string.Empty;
        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public RelayCommand OpenMenuCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand BackToLoginCommand { get; }
        public RelayCommand OpenTableCommand { get; }

        public FloorPlanViewModel(MainViewModel main)
        {
            _main = main;

            // В POS режим "MENU" логично значи "отворѝ сметката/менюто" за избрана маса.
            // Тук го оставяме като безопасен no-op с подсказка.
            OpenMenuCommand = new RelayCommand(_ =>
            {
                Error = "Избери маса и отвори сметка. Менюто се отваря вътре в сметката.";
            });

            OpenTableCommand = new RelayCommand(t =>
            {
                if (t is not TableModel table) return;
                Error = string.Empty;
                _main.Session.CurrentTableId = table.Id;
                _main.Nav.NavigateTo(new TicketViewModel(_main, table.Id));
            });

            LogoutCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });

            BackToLoginCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });

            Load();
        }

        private void Load()
        {
            Error = string.Empty;
            Zones.Clear();
            foreach (var z in _main.FloorRepo.GetZones())
                Zones.Add(z);

            SelectedZone = Zones.Count > 0 ? Zones[0] : null;
            ReloadTables();
        }

        private void ReloadTables()
        {
            Tables.Clear();
            if (SelectedZone?.Tables == null) return;

            foreach (var t in SelectedZone.Tables)
                Tables.Add(t);
        }
    }
}
