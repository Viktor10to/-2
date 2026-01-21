using System;
using System.Collections.ObjectModel;
using Flexi2.Core.MVVM;
using Flexi2.Models;

namespace Flexi2.ViewModels
{
    public sealed class AdminReportsViewModel : ObservableObject
    {
        private readonly MainViewModel _main;

        public ObservableCollection<TurnoverEntry> Entries { get; } = new();

        private decimal _total;
        public decimal Total { get => _total; set => Set(ref _total, value); }

        private string _rangeLabel = "";
        public string RangeLabel { get => _rangeLabel; set => Set(ref _rangeLabel, value); }

        public RelayCommand TodayCommand { get; }
        public RelayCommand WeekCommand { get; }
        public RelayCommand MonthCommand { get; }
        public RelayCommand YearCommand { get; }
        public RelayCommand BackCommand { get; }
        public RelayCommand BackToActivitiesCommand { get; }

        public AdminReportsViewModel(MainViewModel main)
        {
            _main = main;

            TodayCommand = new RelayCommand(_ => LoadToday());
            WeekCommand = new RelayCommand(_ => LoadWeek());
            MonthCommand = new RelayCommand(_ => LoadMonth());
            YearCommand = new RelayCommand(_ => LoadYear());
            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(new AdminViewModel(_main)));
            BackToActivitiesCommand = new RelayCommand(_ =>
            {
                _main.Session.Logout();
                _main.Nav.NavigateTo(new LoginViewModel(_main));
            });

            LoadToday();
        }

        private static (DateTime fromUtc, DateTime toUtc, string label) RangeToday()
        {
            var tz = TimeZoneInfo.Local;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var startLocal = nowLocal.Date;
            var endLocal = startLocal.AddDays(1);
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
            return (fromUtc, toUtc, $"Днес ({startLocal:dd.MM.yyyy})");
        }

        private static (DateTime fromUtc, DateTime toUtc, string label) RangeWeek()
        {
            var tz = TimeZoneInfo.Local;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            // week starts Monday
            var diff = ((int)nowLocal.DayOfWeek + 6) % 7;
            var startLocal = nowLocal.Date.AddDays(-diff);
            var endLocal = startLocal.AddDays(7);
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
            return (fromUtc, toUtc, $"Седмица ({startLocal:dd.MM}–{endLocal.AddDays(-1):dd.MM})");
        }

        private static (DateTime fromUtc, DateTime toUtc, string label) RangeMonth()
        {
            var tz = TimeZoneInfo.Local;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var startLocal = new DateTime(nowLocal.Year, nowLocal.Month, 1);
            var endLocal = startLocal.AddMonths(1);
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
            return (fromUtc, toUtc, $"Месец ({startLocal:MMMM yyyy})");
        }

        private static (DateTime fromUtc, DateTime toUtc, string label) RangeYear()
        {
            var tz = TimeZoneInfo.Local;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var startLocal = new DateTime(nowLocal.Year, 1, 1);
            var endLocal = startLocal.AddYears(1);
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
            return (fromUtc, toUtc, $"Година ({startLocal:yyyy})");
        }

        private void LoadRange((DateTime fromUtc, DateTime toUtc, string label) range)
        {
            RangeLabel = range.label;
            Total = _main.ReportsRepo.GetTurnoverSum(range.fromUtc, range.toUtc);

            Entries.Clear();
            foreach (var e in _main.ReportsRepo.GetTurnoverEntries(range.fromUtc, range.toUtc))
                Entries.Add(e);
        }

        private void LoadToday() => LoadRange(RangeToday());
        private void LoadWeek() => LoadRange(RangeWeek());
        private void LoadMonth() => LoadRange(RangeMonth());
        private void LoadYear() => LoadRange(RangeYear());
    }
}
