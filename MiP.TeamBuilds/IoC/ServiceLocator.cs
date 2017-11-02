using Autofac;

namespace MiP.TeamBuilds.IoC
{
    public static class ServiceLocator
    {
        public static IContainer Instance { get; } = InitContainer();

        private static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            // controls
            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<SettingsWindow>().AsSelf();

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
