using Flexi2.Core.MVVM;
using Flexi2.Core.Navigation;
using Flexi2.Core.Session;

namespace Flexi2.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly UserSession _session;

        private string _pin = "";
        private bool _isAdminMode;

        public bool IsAdminMode
        {
            get => _isAdminMode;
            set
            {
                _isAdminMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModeText));
            }
        }

        public string ModeText => IsAdminMode ? "ADMIN MODE" : "POS MODE";

        public string MaskedPin => new string('●', _pin.Length);

        public RelayCommand SetPosModeCommand { get; }
        public RelayCommand SetAdminModeCommand { get; }

        public RelayCommand<string> AddDigitCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand BackspaceCommand { get; }
        public RelayCommand LoginCommand { get; }

        public LoginViewModel(NavigationService nav, UserSession session)
        {
            _nav = nav;
            _session = session;

            SetPosModeCommand = new RelayCommand(() => IsAdminMode = false);
            SetAdminModeCommand = new RelayCommand(() => IsAdminMode = true);

            AddDigitCommand = new RelayCommand<string>(digit =>
            {
                if (_pin.Length < 6 && digit != null)
                {
                    _pin += digit;
                    OnPropertyChanged(nameof(MaskedPin));
                }
            });

            ClearCommand = new RelayCommand(() =>
            {
                _pin = "";
                OnPropertyChanged(nameof(MaskedPin));
            });

            BackspaceCommand = new RelayCommand(() =>
            {
                if (_pin.Length > 0)
                {
                    _pin = _pin[..^1];
                    OnPropertyChanged(nameof(MaskedPin));
                }
            });

            LoginCommand = new RelayCommand(() =>
            {
                // TEMP логика (после ще е SQLite):
                // POS: 1111
                // ADMIN: 0000
                if (!IsAdminMode && _pin == "1111")
                {
                    _session.IsLoggedIn = true;
                    _session.Role = UserRole.Waiter;
                    _session.DisplayName = "WAITER";

                    _nav.Navigate(new FloorPlanViewModel(_nav, _session));
                    return;
                }

                if (IsAdminMode && _pin == "0000")
                {
                    _session.IsLoggedIn = true;
                    _session.Role = UserRole.Admin;
                    _session.DisplayName = "ADMIN";

                    _nav.Navigate(new AdminViewModel(_nav, _session));
                    return;
                }

                // грешен PIN → чистим
                _pin = "";
                OnPropertyChanged(nameof(MaskedPin));
            });
        }
    }
}
