using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class FloorPlanViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<ZoneModel> Zones { get; } = new();
        public ObservableCollection<TableModel> Tables { get; } = new();

        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set
            {
                _selectedZone = value;
                OnPropertyChanged();
                LoadTables();
            }
        }

        public string Title => $"POS • {_main.Session.CurrentUser?.Name}";
        public RelayCommand LogoutCommand { get; }
        public RelayCommand OpenMenuCommand { get; }
        public RelayCommand OpenTableCommand { get; }

        public FloorPlanViewModel(MainViewModel main)
        {
            _main = main;

            LogoutCommand = new RelayCommand(_ => _main.LogoutCommand.Execute(null));
            OpenMenuCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new CategoryViewModel(_main)));
            OpenTableCommand = new RelayCommand(t => OpenTable((TableModel)t!));

            LoadZones();
        }

        private void LoadZones()
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

        private void OpenTable(TableModel table)
        {
            var me = _main.Session.CurrentUser;
            if (me == null) return;

            // POS няма право в чужда маса
            if (me.Role != UserRole.Admin &&
                table.Status == TableStatus.Occupied &&
                table.OwnerUserId.HasValue &&
                table.OwnerUserId.Value != me.Id)
                return;

            _main.Session.CurrentTableId = table.Id;
            _main.Nav.NavigateTo(new TicketViewModel(_main, table.Id));
        }

    }
}
