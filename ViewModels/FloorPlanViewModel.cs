using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;
using Flexi2.Models;
using System.Collections.ObjectModel;

namespace Flexi2.ViewModels
{
    public class FloorPlanViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        public string Title => $"FLOOR PLAN – {_session.DisplayName}";

        public ObservableCollection<TableModel> Tables { get; }

        public RelayCommand<TableModel> TableClickCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public FloorPlanViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            // ТЕСТОВИ МАСИ
            Tables = new ObservableCollection<TableModel>
            {
                new TableModel { Number = 1, X = 50,  Y = 50,  Status = TableStatus.Free },
                new TableModel { Number = 2, X = 200, Y = 50,  Status = TableStatus.Busy },
                new TableModel { Number = 3, X = 50,  Y = 200, Status = TableStatus.Free },
                new TableModel { Number = 4, X = 200, Y = 200, Status = TableStatus.Free }
            };

            TableClickCommand = new RelayCommand<TableModel>(table =>
            {
                if (table != null)
                {
                    // засега само сменяме статус (визуален ефект)
                    table.Status = table.Status == TableStatus.Free
                        ? TableStatus.Busy
                        : TableStatus.Free;

                    OnPropertyChanged(nameof(Tables));
                }
            });

            LogoutCommand = new RelayCommand(() =>
            {
                _session.Logout();
                _nav.Navigate(new LoginViewModel(_nav, _session));
            });
        }
    }
}
