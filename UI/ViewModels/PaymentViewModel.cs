using Flexi2.Core.MVVM;
using System;
using System.Globalization;
using System.Linq;

namespace Flexi2.ViewModels
{
    public sealed class PaymentViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        private readonly TicketViewModel _ticket;

        public int TableId => _ticket.TableId;

        private string _paymentMethod = "Cash";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (Set(ref _paymentMethod, value))
                {
                    ConfirmCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _discountPercentText = "0";
        public string DiscountPercentText
        {
            get => _discountPercentText;
            set
            {
                if (Set(ref _discountPercentText, value))
                    Recalc();
            }
        }

        private string _tipText = "0";
        public string TipText
        {
            get => _tipText;
            set
            {
                if (Set(ref _tipText, value))
                    Recalc();
            }
        }

        private decimal _subtotal;
        public decimal Subtotal { get => _subtotal; set => Set(ref _subtotal, value); }

        private decimal _total;
        public decimal Total { get => _total; set => Set(ref _total, value); }

        private string _error = "";
        public string Error { get => _error; set => Set(ref _error, value); }

        public RelayCommand BackCommand { get; }
        public RelayCommand CashCommand { get; }
        public RelayCommand CardCommand { get; }
        public RelayCommand OtherCommand { get; }
        public RelayCommand ConfirmCommand { get; }

        public PaymentViewModel(MainViewModel main, TicketViewModel ticket)
        {
            _main = main;
            _ticket = ticket;

            BackCommand = new RelayCommand(_ => _main.Nav.NavigateTo(_ticket));
            CashCommand = new RelayCommand(_ => PaymentMethod = "Cash");
            CardCommand = new RelayCommand(_ => PaymentMethod = "Card");
            OtherCommand = new RelayCommand(_ => PaymentMethod = "Other");

            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm());

            // Use current ticket total (locked + draft). If draft exists, user must place it first.
            Subtotal = _ticket.Total;
            Recalc();
        }

        private bool CanConfirm()
        {
            // Don't allow close if there are draft items not placed.
            return _ticket.DraftItems.Count == 0 && !string.IsNullOrWhiteSpace(PaymentMethod);
        }

        private void Recalc()
        {
            Subtotal = _ticket.Total;

            var dp = ParseDecimal(DiscountPercentText);
            if (dp < 0) dp = 0;
            if (dp > 100) dp = 100;

            var tip = ParseDecimal(TipText);
            if (tip < 0) tip = 0;

            var discount = Subtotal * (dp / 100m);
            Total = (Subtotal - discount) + tip;
            ConfirmCommand.RaiseCanExecuteChanged();
        }

        private static decimal ParseDecimal(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;
            // Accept both comma and dot.
            var norm = text.Trim().Replace(',', '.');
            if (decimal.TryParse(norm, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                return v;
            return 0m;
        }

        private void Confirm()
        {
            Error = "";

            if (_ticket.DraftItems.Count > 0)
            {
                Error = "Имате непоръчани (Draft) артикули. Натиснете 'Поръчай' първо.";
                return;
            }

            var dp = ParseDecimal(DiscountPercentText);
            if (dp < 0) dp = 0;
            if (dp > 100) dp = 100;

            var tip = ParseDecimal(TipText);
            if (tip < 0) tip = 0;

            _main.OrderRepo.CloseOrder(TableId, PaymentMethod, dp, tip);
            _main.FloorRepo.SetTableFree(TableId);

            _main.Session.CurrentTableId = null;
            _main.Nav.NavigateTo(new FloorPlanViewModel(_main));
        }
    }
}
