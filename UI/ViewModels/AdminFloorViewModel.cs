using Flexi2.Core.MVVM;
using Flexi2.Models;
using System;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public sealed class AdminFloorViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<ZoneModel> Zones { get; } = new();
        public ObservableCollection<TableModel> Tables { get; } = new();

        private TableModel? _selectedTable;
        public TableModel? SelectedTable
        {
            get => _selectedTable;
            set => SetProperty(ref _selectedTable, value);
        }

        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set
            {
                if (SetProperty(ref _selectedZone, value))
                    ReloadTables();
            }
        }

        private string _newZoneName = string.Empty;
        public string NewZoneName
        {
            get => _newZoneName;
            set => SetProperty(ref _newZoneName, value);
        }

        private string _newTableName = string.Empty;
        public string NewTableName
        {
            get => _newTableName;
            set => SetProperty(ref _newTableName, value);
        }

        private string _error = string.Empty;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public RelayCommand BackCommand { get; }
        public RelayCommand BackToActivitiesCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public RelayCommand AddZoneCommand { get; }
        public RelayCommand DeleteZoneCommand { get; }
        public RelayCommand AddTableCommand { get; }
        public RelayCommand DeleteTableCommand { get; }

        public AdminFloorViewModel(MainViewModel main)
        {
            _main = main;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminViewModel(_main)));
            BackToActivitiesCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });
            RefreshCommand = new RelayCommand(_ => Load());

            AddZoneCommand = new RelayCommand(_ => AddZone());
            DeleteZoneCommand = new RelayCommand(_ => DeleteZone());
            AddTableCommand = new RelayCommand(_ => AddTable());
            DeleteTableCommand = new RelayCommand(t => DeleteTable(t as TableModel));

            Load();
        }

        private void Load()
        {
            Error = string.Empty;
            Zones.Clear();
            foreach (var z in _main.FloorRepo.GetZones())
                Zones.Add(z);

            SelectedZone ??= Zones.Count > 0 ? Zones[0] : null;
            ReloadTables();
        }

        private void ReloadTables()
        {
            Tables.Clear();
            if (SelectedZone?.Tables == null) return;
            foreach (var t in SelectedZone.Tables)
                Tables.Add(t);

            SelectedTable = Tables.Count > 0 ? Tables[0] : null;
        }

        // Called from the view (drag & drop). Persists to DB.
        public void SaveTableLayout(TableModel table)
        {
            try
            {
                _main.FloorRepo.UpdateTableLayout(table.Id, table.PosX, table.PosY, table.Width, table.Height);
                Error = string.Empty;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void AddZone()
        {
            try
            {
                Error = string.Empty;
                var name = (NewZoneName ?? string.Empty).Trim();
                if (name.Length == 0)
                {
                    Error = "Въведи име на зона.";
                    return;
                }

                _main.FloorRepo.AddZone(name);
                NewZoneName = string.Empty;
                Load();

                // select the newly added zone
                foreach (var z in Zones)
                {
                    if (string.Equals(z.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedZone = z;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void DeleteZone()
        {
            try
            {
                Error = string.Empty;
                if (SelectedZone == null)
                {
                    Error = "Избери зона.";
                    return;
                }

                _main.FloorRepo.DeleteZone(SelectedZone.Id);
                SelectedZone = null;
                Load();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void AddTable()
        {
            try
            {
                Error = string.Empty;
                if (SelectedZone == null)
                {
                    Error = "Избери зона.";
                    return;
                }

                var name = (NewTableName ?? string.Empty).Trim();
                if (name.Length == 0)
                {
                    Error = "Въведи име на маса.";
                    return;
                }

                _main.FloorRepo.AddTable(SelectedZone.Id, name);
                NewTableName = string.Empty;
                // reload zones so SelectedZone.Tables is refreshed
                var keepZoneId = SelectedZone.Id;
                Load();
                foreach (var z in Zones)
                    if (z.Id == keepZoneId) { SelectedZone = z; break; }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private void DeleteTable(TableModel? table)
        {
            try
            {
                Error = string.Empty;
                if (table == null)
                {
                    Error = "Избери маса.";
                    return;
                }

                _main.FloorRepo.DeleteTable(table.Id);

                var keepZoneId = SelectedZone?.Id;
                Load();
                if (keepZoneId != null)
                    foreach (var z in Zones)
                        if (z.Id == keepZoneId) { SelectedZone = z; break; }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

    }
}
