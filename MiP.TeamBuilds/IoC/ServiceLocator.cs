using Autofac;

using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Main;
using MiP.TeamBuilds.UI.Settings;

namespace MiP.TeamBuilds.IoC
{
    public static class ServiceLocator
    {
        public static IContainer Instance { get; } = InitContainer();

        private static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            // controls
            builder.RegisterType<UI.Main.MainWindow>().AsSelf();
            builder.RegisterType<UI.Settings.SettingsWindow>().AsSelf();

            // viewmodel
            builder.RegisterType<MainViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();

            // commands
            builder.RegisterType<ShowSettingsCommand>().AsSelf();

            // helpers
            builder.RegisterType<TfsBuildHelper>().AsSelf();

            IContainer container = builder.Build();

            return container;
        }

    }
}
