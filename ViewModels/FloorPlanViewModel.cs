using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Models;
using Flexi2.ViewModels.Orders;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;



namespace Flexi2.ViewModels
{
    public class FloorPlanViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        public ICommand OpenTableCommand { get; }

        public string Title => $"FLOOR PLAN – {_session.DisplayName}";

        public ObservableCollection<ZoneModel> Zones { get; }
        private ZoneModel? _selectedZone;
        public ZoneModel? SelectedZone
        {
            get => _selectedZone;
            set { _selectedZone = value; OnPropertyChanged(); LoadTablesForZone(); }
        }

        public ObservableCollection<TableModel> Tables { get; } = new();

        public RelayCommand<TableModel> TableClickCommand { get; }
        public RelayCommand LogoutCommand { get; }

        // демо: таблици по зони (после → SQLite)
        private readonly ObservableCollection<TableModel> _allTables;

        public FloorPlanViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            OpenTableCommand = new RelayCommand<TableModel>(table =>
            {
                table.Status = TableStatus.Busy;
                _nav.Navigate(new OrderViewModel(_nav, _session, table));
            });



            // DEMO маси (ако вече ги имаш – не ги дублирай)
            Tables = new ObservableCollection<TableModel>
            {
                new TableModel { Number = 1, Status = TableStatus.Free },
                new TableModel { Number = 2, Status = TableStatus.Busy },
                new TableModel { Number = 3, Status = TableStatus.Free },
                new TableModel { Number = 4, Status = TableStatus.Free }
            };
        }
        private void OpenTable(TableModel table)
        {
            if (table == null) return;

            _nav.Navigate(new OrderViewModel(_nav, _session, table));
        }



        private void LoadTablesForZone()
        {
            Tables.Clear();
            if (SelectedZone == null) return;

            // демо: първите 4 са Салон, другите 2 са Градина
            var list = SelectedZone.Id == 1
                ? _allTables.Take(4)
                : _allTables.Skip(4);

            foreach (var t in list)
                Tables.Add(t);
        }
    }
}
