using System.Windows;

namespace Flexi2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // IMPORTANT: StartupUri is removed from App.xaml.
            // We create/show the main window here to avoid "opens twice".
            var window = new MainWindow();
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
        }
    }
}
