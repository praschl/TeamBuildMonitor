using System;
using Autofac;

using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using MiP.TeamBuilds.UI.Settings;
using ToastNotifications;
using System.Windows;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Ambient;
using Autofac.Features.AttributeFilters;

namespace MiP.TeamBuilds.IoC
{
    public static class ServiceLocator
    {
        public static IContainer Instance { get; } = InitContainer();

        private static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            // controls
            builder.RegisterType<AmbientWindow>().AsSelf();
            builder.RegisterType<SettingsWindow>().AsSelf();

            // viewmodel
            builder.RegisterType<KnownBuildsViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<AmbientViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SettingsViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();

            // commands
            builder.RegisterType<ShowSettingsCommand>().AsSelf().SingleInstance();
            builder.RegisterType<SaveSettingsCommand>().AsSelf().SingleInstance();
            builder.RegisterType<RestartTimerCommand>().AsSelf().SingleInstance();
            builder.RegisterType<QuitCommand>().AsSelf().SingleInstance();

            // helpers
            builder.RegisterType<BuildInfoProvider>().Keyed<IBuildInfoProvider>("http").AsImplementedInterfaces();
            builder.RegisterType<TestBuildInfoProvider>().Keyed<IBuildInfoProvider>("demo").AsImplementedInterfaces();
            builder.RegisterType<BuildInfoProviderFactory>().AsSelf().WithAttributeFiltering();

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
