using System;
using System.Collections.ObjectModel;
using System.Linq;
using FlexiPOS.Core;
using FlexiPOS.Models;
using FlexiPOS.Services;

namespace FlexiPOS.ViewModels
{
    public sealed class FloorPlanViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly SessionContext _session;

        public FloorPlanViewModel(NavigationService nav, SessionContext session)
        {
            _nav = nav;
            _session = session;

            Zones = new ObservableCollection<ZoneModel>
            {
                new ZoneModel{ Id=1, Name="Салон" },
                new ZoneModel{ Id=2, Name="Градина" }
            };

            // DEMO маси (после CRUD от Admin)
            AllTables = new ObservableCollection<TableModel>
            {
                new TableModel{ Id=1, Name="T1", ZoneId=1, Shape=TableShape.Rectangle, Status=TableStatus.Free },
                new TableModel{ Id=2, Name="T2", ZoneId=1, Shape=TableShape.Circle, Status=TableStatus.Occupied, OwnerUserId=1, CurrentTotal=24.50m, OpenedAt=DateTime.Now.AddMinutes(-18) },
                new TableModel{ Id=3, Name="T3", ZoneId=2, Shape=TableShape.Rectangle, Status=TableStatus.Free },
                new TableModel{ Id=4, Name="T4", ZoneId=2, Shape=TableShape.Circle, Status=TableStatus.Occupied, OwnerUserId=2, CurrentTotal=71.00m, OpenedAt=DateTime.Now.AddMinutes(-62) }
            };

            SelectZoneCommand = new RelayCommand(z => SelectZone((ZoneModel)z!));
            OpenTableCommand = new RelayCommand(t => OpenTable((TableModel)t!));

            SelectedZone = Zones.First();
            RefreshFilteredTables();
        }

        public ObservableCollection<ZoneModel> Zones { get; }
        public ObservableCollection<TableModel> AllTables { get; }

        private ObservableCollection<TableModel> _filteredTables = new();
        public ObservableCollection<TableModel> FilteredTables
        {
            get => _filteredTables;
            set => Set(ref _filteredTables, value);
        }

        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set
            {
                if (Set(ref _selectedZone, value))
                    RefreshFilteredTables();
            }
        }

        public string HeaderText => $"{_session.DisplayName} • {_session.Role}";

        public RelayCommand SelectZoneCommand { get; }
        public RelayCommand OpenTableCommand { get; }

        private string _toast = "";
        public string Toast
        {
            get => _toast;
            set => Set(ref _toast, value);
        }

        private void SelectZone(ZoneModel zone) => SelectedZone = zone;

        private void RefreshFilteredTables()
        {
            if (SelectedZone == null) return;
            FilteredTables = new ObservableCollection<TableModel>(
                AllTables.Where(t => t.ZoneId == SelectedZone.Id));
        }

        private void OpenTable(TableModel table)
        {
            // правило: сервитьор не може да отваря чужда маса
            if (_session.Role == Models.UserRole.PosWaiter)
            {
                if (table.Status == TableStatus.Occupied && table.OwnerUserId != _session.UserId)
                {
                    Toast = "Нямаш достъп до маса на друг сервитьор.";
                    return;
                }
            }

            Toast = "";

            // Етап 1: само показваме, че работи
            // Етап 2: навигация към CategoriesViewModel
            _nav.NavigateTo(new CategoriesViewModel(_nav, _session, table));
        }
    }
}
