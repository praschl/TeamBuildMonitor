using Autofac;
using Microsoft.Shell;
using MiP.TeamBuilds.IoC;
using System.Windows;
using System.Collections.Generic;
using MiP.TeamBuilds.UI.Ambient;

namespace MiP.TeamBuilds
{
    public partial class App : Application, ISingleInstanceApp
    {
        private const string UniqueApplicationKey = "MiP.TeamBuilds.App.Unique_997ab5db-3580-4446-b16e-58cf2acdc9f6";

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(UniqueApplicationKey))
            {
                var window = ServiceLocator.Instance.Resolve<AmbientWindow>();

                // start in hidden mode
                window.Hide();
            }
            else
            {
                Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SingleInstance<App>.Cleanup();
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // application was opened a second time.
            // this method is called in the first instance of the application with the args that were passed to the second.

            // lets bring the main window into view, when another instance was started

            //App.Current.MainWindow.WindowState = WindowState.Normal;
            //App.Current.MainWindow.Visibility = Visibility.Visible;
            //App.Current.MainWindow.BringIntoView();

            return true;
        }
    }
}
