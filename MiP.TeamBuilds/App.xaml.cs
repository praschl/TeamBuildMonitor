using Autofac;
using MiP.TeamBuilds.IoC;
using System.Windows;

namespace MiP.TeamBuilds
{
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // TODO: Allow only one instance of Application running

            var window = ServiceLocator.Instance.Resolve<UI.Main.MainWindow>();

            // start in hidden mode
            window.Hide();
        }
    }
}
