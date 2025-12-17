using Flexi2.Core.MVVM;
using Flexi2.Core.Session;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flexi2.ViewModels
{
    public class HourTurnover
    {
        public int Hour { get; set; }          // 0–23
        public decimal Amount { get; set; }    // оборот за часа
    }

    public class AdminViewModel : ObservableObject
    {
        private readonly UserSession _session;

        public ObservableCollection<HourTurnover> HourlyTurnover { get; }
            = new ObservableCollection<HourTurnover>();

        public decimal TotalTurnover => _session.TotalTurnover;

        public AdminViewModel(UserSession session)
        {
            _session = session;
            Recalculate();
        }

        public void Recalculate()
        {
            HourlyTurnover.Clear();

            var grouped = _session.TurnoverHistory
                .GroupBy(t => t.Time.Hour)
                .OrderBy(g => g.Key);

            foreach (var g in grouped)
            {
                HourlyTurnover.Add(new HourTurnover
                {
                    Hour = g.Key,
                    Amount = g.Sum(x => x.Amount)
                });
            }

            OnPropertyChanged(nameof(TotalTurnover));
        }
    }
}
