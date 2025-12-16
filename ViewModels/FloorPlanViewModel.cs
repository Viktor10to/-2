using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Models;
using Flexi2.ViewModels.Orders;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flexi2.ViewModels
{
    public class FloorPlanViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

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

            Zones = new ObservableCollection<ZoneModel>
            {
                new ZoneModel{ Id=1, Name="Салон" },
                new ZoneModel{ Id=2, Name="Градина" }
            };

            _allTables = new ObservableCollection<TableModel>
            {
                new TableModel{ Number=1, X=40,  Y=40,  Status=TableStatus.Free },
                new TableModel{ Number=2, X=180, Y=40,  Status=TableStatus.Busy },
                new TableModel{ Number=3, X=40,  Y=180, Status=TableStatus.Free },
                new TableModel{ Number=4, X=180, Y=180, Status=TableStatus.Free },

                new TableModel{ Number=5, X=40,  Y=40,  Status=TableStatus.Free },
                new TableModel{ Number=6, X=180, Y=40,  Status=TableStatus.Free }
            };

            SelectedZone = Zones.First();

            TableClickCommand = new RelayCommand<TableModel>(table =>
            {
                if (table != null)
                    _nav.Navigate(new OrderViewModel());
            });

            LogoutCommand = new RelayCommand(() =>
            {
                _session.Logout();
                _nav.Navigate(new LoginViewModel(_nav, _session));
            });
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
