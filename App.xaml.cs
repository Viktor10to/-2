using System;
using System.Windows;

namespace Flexi2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var wnd = new MainWindow();
                MainWindow = wnd;
                wnd.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup crash");
                Shutdown();
            }
        }
    }
}
