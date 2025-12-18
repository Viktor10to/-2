using System;
using System.Collections.ObjectModel;
using System.Linq;
using FlexiPOS.Core;
using FlexiPOS.Models;
using FlexiPOS.Services;

namespace FlexiPOS.ViewModels
{
    public sealed class LoginViewModel : ObservableObject
    {
        private readonly NavigationService _nav;

        public LoginViewModel(NavigationService nav)
        {
            _nav = nav;

            NumpadButtons = new ObservableCollection<string>(
                new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "←" });

            PressKeyCommand = new RelayCommand(p => PressKey(p?.ToString() ?? ""));
            LoginCommand = new RelayCommand(Login, CanLogin);

            // demo PIN-и
            PosPin = "";
            AdminPin = "";
            SelectedMode = 0; // 0 POS, 1 ADMIN
        }

        // 0 = POS, 1 = ADMIN
        private int _selectedMode;
        public int SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (Set(ref _selectedMode, value))
                    RaiseLoginCanExecute();
            }
        }

        private string _posPin = "";
        public string PosPin
        {
            get => _posPin;
            set { if (Set(ref _posPin, value)) RaiseLoginCanExecute(); }
        }

        private string _adminPin = "";
        public string AdminPin
        {
            get => _adminPin;
            set { if (Set(ref _adminPin, value)) RaiseLoginCanExecute(); }
        }

        private string _error = "";
        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public ObservableCollection<string> NumpadButtons { get; }
        public RelayCommand PressKeyCommand { get; }
        public RelayCommand LoginCommand { get; }

        private string ActivePin
        {
            get => SelectedMode == 0 ? PosPin : AdminPin;
            set
            {
                if (SelectedMode == 0) PosPin = value;
                else AdminPin = value;
            }
        }

        private void PressKey(string key)
        {
            Error = "";

            if (key == "C")
            {
                ActivePin = "";
                return;
            }
            if (key == "←")
            {
                if (ActivePin.Length > 0)
                    ActivePin = ActivePin.Substring(0, ActivePin.Length - 1);
                return;
            }

            if (key.All(char.IsDigit))
            {
                if (ActivePin.Length >= 8) return; // лимит
                ActivePin += key;
            }
        }

        private bool CanLogin()
        {
            var pin = ActivePin;
            return !string.IsNullOrWhiteSpace(pin) && pin.Length >= 4;
        }

        private void Login()
        {
            // TODO: после -> SQLite Users
            if (SelectedMode == 0)
            {
                // POS demo: PIN 1111
                if (PosPin != "1111")
                {
                    Error = "Грешен POS PIN.";
                    return;
                }

                var session = new SessionContext
                {
                    Role = UserRole.PosWaiter,
                    UserId = 1,
                    DisplayName = "Waiter #1"
                };

                _nav.NavigateTo(new FloorPlanViewModel(_nav, session));
            }
            else
            {
                // ADMIN demo: PIN 9999
                if (AdminPin != "9999")
                {
                    Error = "Грешен ADMIN PIN.";
                    return;
                }

                var session = new SessionContext
                {
                    Role = UserRole.Admin,
                    UserId = 999,
                    DisplayName = "ADMIN"
                };

                _nav.NavigateTo(new FloorPlanViewModel(_nav, session));
            }
        }

        private void RaiseLoginCanExecute() => LoginCommand.RaiseCanExecuteChanged();
    }

    public sealed class SessionContext
    {
        public UserRole Role { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; } = "";
    }
}
