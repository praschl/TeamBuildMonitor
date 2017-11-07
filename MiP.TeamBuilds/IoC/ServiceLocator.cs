using System;
using Autofac;

using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Main;
using MiP.TeamBuilds.UI.Settings;
using ToastNotifications;
using System.Windows;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using MiP.TeamBuilds.UI.Commands;

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
            builder.RegisterType<KnownBuildsModel>().AsSelf().AsImplementedInterfaces().SingleInstance();

            // commands
            builder.RegisterType<ShowSettingsCommand>().AsSelf();

            // helpers
            builder.RegisterType<BuildInfoProvider>().AsSelf();

            RegisterNotifier(builder);

            return builder.Build();
        }

        private static void RegisterNotifier(ContainerBuilder builder)
        {
            var notifier = new Notifier(cfg =>
            {
                var offset = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

                cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 0, offset);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(10),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(10));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });

            builder.RegisterInstance(notifier).AsSelf().SingleInstance();
        }
    }
}
