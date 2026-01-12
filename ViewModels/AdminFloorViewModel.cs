using System;
using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class AdminFloorViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<ZoneModel> Zones { get; } = new();
        public ObservableCollection<TableModel> Tables { get; } = new();

        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set { _selectedZone = value; OnPropertyChanged(); LoadTables(); }
        }

        private string _newZoneName = "";
        public string NewZoneName { get => _newZoneName; set { _newZoneName = value; OnPropertyChanged(); } }

        private string _newTableName = "";
        public string NewTableName { get => _newTableName; set { _newTableName = value; OnPropertyChanged(); } }

        private string _error = "";
        public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

        public RelayCommand AddZoneCommand { get; }
        public RelayCommand DeleteZoneCommand { get; }
        public RelayCommand AddTableCommand { get; }
        public RelayCommand DeleteTableCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public AdminFloorViewModel(MainViewModel main)
        {
            _main = main;

            AddZoneCommand = new RelayCommand(_ => Safe(AddZone));
            DeleteZoneCommand = new RelayCommand(_ => Safe(DeleteZone));
            AddTableCommand = new RelayCommand(_ => Safe(AddTable));
            DeleteTableCommand = new RelayCommand(t => Safe(() => DeleteTable((TableModel)t!)));
            RefreshCommand = new RelayCommand(_ => Safe(Load));

            Load();
        }

        private void Safe(Action act)
        {
            try
            {
                Error = "";
                act();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void Load()
        {
            Zones.Clear();
            foreach (var z in _main.FloorRepo.GetZones())
                Zones.Add(z);

            SelectedZone = Zones.Count > 0 ? Zones[0] : null;
        }

        private void LoadTables()
        {
            Tables.Clear();
            if (SelectedZone == null) return;

            foreach (var t in _main.FloorRepo.GetTablesByZone(SelectedZone.Id))
                Tables.Add(t);
        }

        private void AddZone()
        {
            if (string.IsNullOrWhiteSpace(NewZoneName)) return;
            _main.FloorRepo.CreateZone(NewZoneName.Trim());
            _main.AuditRepo.Log(_main.Session.CurrentUser!.Id, "CREATE", "Zone", 0, NewZoneName.Trim());
            NewZoneName = "";
            Load();
        }

        private void DeleteZone()
        {
            if (SelectedZone == null) return;

            // ✅ safe delete (deletes tables first)
            _main.FloorRepo.DeleteZoneSafe(SelectedZone.Id);
            _main.AuditRepo.Log(_main.Session.CurrentUser!.Id, "DELETE", "Zone", SelectedZone.Id, SelectedZone.Name);
            Load();
        }

        private void AddTable()
        {
            if (SelectedZone == null) return;
            if (string.IsNullOrWhiteSpace(NewTableName)) return;

            _main.FloorRepo.CreateTable(SelectedZone.Id, NewTableName.Trim());
            _main.AuditRepo.Log(_main.Session.CurrentUser!.Id, "CREATE", "Table", 0, $"Zone={SelectedZone.Name}, Name={NewTableName.Trim()}");
            NewTableName = "";
            LoadTables();
        }

        private void DeleteTable(TableModel tb)
        {
            _main.FloorRepo.DeleteTable(tb.Id);
            _main.AuditRepo.Log(_main.Session.CurrentUser!.Id, "DELETE", "Table", tb.Id, tb.Name);
            LoadTables();
        }
    }
}
