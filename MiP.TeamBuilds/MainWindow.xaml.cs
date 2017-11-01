using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using Microsoft.TeamFoundation.Build.Client;
using MiP.TeamBuilds.Properties;

namespace MiP.TeamBuilds
{
    public partial class MainWindow : Window
    {
        private readonly Notifier _notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 0, 50);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(10),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private TfsBuildHelper _tfsBuildHelper;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            StartTimer();
        }

        private void OpenSettings()
        {
            var settings = new SettingsWindow();
            settings.ShowDialog();
        }

        private bool TestTfsUri()
        {
            try
            {
                new Uri(Settings.Default.TfsUrl);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void StartTimer()
        {
            while (!TestTfsUri())
                OpenSettings();

            _tfsBuildHelper?.Dispose();
            _tfsBuildHelper = new TfsBuildHelper();

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private Dictionary<int, BuildInfo> _lastBuilds = new Dictionary<int, BuildInfo>();

        private void Timer_Tick(object sender, EventArgs e)
        {
            var currentBuilds = _tfsBuildHelper.GetCurrentBuilds().ToList();

            foreach (var build in currentBuilds)
            {
                if (!build.Finished)
                    ShowBuildStarted(build);
                else
                    ShowBuildFinished(build);
            }
        }

        private void ShowBuildFinished(BuildInfo build)
        {
            if (!_lastBuilds.ContainsKey(build.Id))
                return; // was already shown as finished, or at least, never shown as started (when application started after build finished).

            var message = $"Build {build.Status}: {build.BuildDefinitionName}";
            switch (build.Status)
            {
                case BuildStatus.Failed:
                    _notifier.ShowError(message);
                    break;

                case BuildStatus.PartiallySucceeded:
                case BuildStatus.Stopped:
                    _notifier.ShowWarning(message);
                    break;

                case BuildStatus.Succeeded:
                    _notifier.ShowSuccess(message);
                    break;
            }

            _lastBuilds.Remove(build.Id);
        }

        private void ShowBuildStarted(BuildInfo build)
        {
            if (_lastBuilds.ContainsKey(build.Id))
                return; // was already shown as started.

            var message = $"Build started: {build.BuildDefinitionName}";

            _notifier.ShowInformation(message);

            _lastBuilds.Add(build.Id, build);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            OpenSettings();
                       
            StartTimer();
        }
    }
}
