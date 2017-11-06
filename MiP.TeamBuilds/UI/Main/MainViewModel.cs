using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Settings;

using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using MiP.TeamBuilds.UI.Notifications;
using System.Diagnostics;

namespace MiP.TeamBuilds.UI.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TfsBuildHelper _tfsBuildHelper;
        private readonly Dictionary<int, BuildInfo> _lastKnownBuilds = new Dictionary<int, BuildInfo>();

        private readonly Notifier _notifier = new Notifier(cfg =>
        {
            var offset = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 0, offset);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(10),
                maximumNotificationCount: MaximumNotificationCount.FromCount(10));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        private readonly MessageOptions _defaultOptions = new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true,
            ShowCloseButton = false,
            NotificationClickAction = n => n.Close()
        };
        private readonly Func<ShowSettingsCommand> _showSettingsCommandFactory;

        public MainViewModel(Func<ShowSettingsCommand> showSettingsCommandFactory)
        {
            _showSettingsCommandFactory = showSettingsCommandFactory;
        }

        public void Initialize()
        {
            RestartTimer();
        }

        [Conditional("DEBUG")]
        private void DebugTestNotification()
        {
            var build = new BuildInfo(null)
            {
                Status = BuildStatus.PartiallySucceeded,
                BuildDefinitionName = "MyBuild",
                BuildSummary = new Uri("http://google.com"),
                DropLocation = "C:\\",
                RequestedBy = "Myself",
                QueuedTime = DateTime.Now.AddMinutes(-23).AddSeconds(14),
                FinishTime = DateTime.Now
            };
            _notifier.ShowInformation(new BuildMessage(build), _defaultOptions);
            _notifier.ShowSuccess(new BuildSuccessMessage(build), _defaultOptions);
            _notifier.ShowWarning(new BuildFailureMessage(build), _defaultOptions);
            _notifier.ShowError(new BuildFailureMessage(build), _defaultOptions);
        }

        public void RestartTimer()
        {
            DebugTestNotification();

            var uri = CreateTfsUri();
            if (uri == null)
                return;

            _tfsBuildHelper?.Dispose();
            _tfsBuildHelper = new TfsBuildHelper(uri);

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private Uri CreateTfsUri()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.TfsUrl))
            {
                ShowTfsUrlNotSet();
                return null;
            }

            try
            {
                return new Uri(Properties.Settings.Default.TfsUrl);
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return null;
            }
        }

        private void ShowTfsUrlNotSet()
        {
            var displayOptions = new MessageOptions
            {
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = true,
            };

            var content = new TextWithLinkMessage("Setup", "Uri to TFS has not been set yet.", "Go to settings...",
                n =>
                {
                    n.Close();
                    ShowSettingsCommand.Execute(null);
                });

            _notifier.ShowInformation(content, displayOptions);
        }

        private void ShowException(Exception ex)
        {
            var errorDisplayOptions = new MessageOptions
            {
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = true
            };

            _notifier.ShowError(new ExceptionMessage("Exception", ex, _notifier), errorDisplayOptions);
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var buildInfos = await _tfsBuildHelper.GetCurrentBuildsAsync();

                foreach (var build in buildInfos)
                {
                    if (_lastKnownBuilds.ContainsKey(build.Id))
                        continue; // we know that build already and we are connected to it.

                    _lastKnownBuilds.Add(build.Id, build);

                    NotifyBuild(build);

                    build.Connect();
                    build.BuildUpdated += Build_BuildUpdated;
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void Build_BuildUpdated(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var build = (BuildInfo)sender;
                NotifyBuild(build);

                if (build.PollingException != null)
                {
                    ShowException(build.PollingException);
                }
            });
        }

        private void NotifyBuild(BuildInfo build)
        {
            var title = build.BuildDefinitionName;
            var message = $"Build {build.Status}";

            switch (build.Status)
            {
                case BuildStatus.Failed:
                    _notifier.ShowError(new BuildFailureMessage(build), _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.PartiallySucceeded:
                case BuildStatus.Stopped:
                    _notifier.ShowWarning(new BuildFailureMessage(build), _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.Succeeded:
                    _notifier.ShowSuccess(new BuildSuccessMessage(build), _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.None:
                case BuildStatus.NotStarted:
                case BuildStatus.InProgress:
                    _notifier.ShowInformation(new BuildMessage(build), _defaultOptions);
                    break;
            }
        }

        private void FinalizeBuild(BuildInfo build)
        {
            build.Disconnect();
            build.BuildUpdated -= Build_BuildUpdated;
            _lastKnownBuilds.Remove(build.Id);
        }
        
        public ICommand ShowSettingsCommand => _showSettingsCommandFactory();

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
