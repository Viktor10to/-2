using System;
using System.Windows;

namespace Flexi2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Хваща ВСИЧКИ UI грешки
            this.DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(
                    ex.Exception.ToString(),
                    "CRASH",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ex.Handled = true;
            };

            // Хваща non-UI грешки
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show(
                    ex.ExceptionObject.ToString(),
                    "FATAL CRASH",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            base.OnStartup(e);
            new MainWindow().Show();
        }
    }
}
