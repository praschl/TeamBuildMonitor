using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;

using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System.Windows.Threading;
using System.Windows.Input;
using MiP.TeamBuilds.UI.Commands;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class KnownBuildsViewModel : IRefreshBuildsTimer
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Dictionary<int, BuildInfo> _lastKnownBuilds = new Dictionary<int, BuildInfo>();
        private readonly Notifier _notifier;
        private BuildInfoProvider _buildInfoProvider;

        private readonly MessageOptions _defaultOptions = new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true,
            ShowCloseButton = false,
            NotificationClickAction = n => n.Close()
        };

        private readonly BuildInfoProvider.Factory _buildInfoProviderFactory;

        public KnownBuildsViewModel(Notifier notifier, ShowSettingsCommand showSettingsCommand, BuildInfoProvider.Factory buildInfoProviderFactory)
        {
            _notifier = notifier;
            ShowSettingsCommand = showSettingsCommand;
            _buildInfoProviderFactory = buildInfoProviderFactory;
        }

        public ObservableCollection<BuildInfo> Builds { get; } = new ObservableCollection<BuildInfo>();

        public ICommand ShowSettingsCommand { get; }

        internal void Initialize()
        {
            RestartTimer();
        }

        public void RestartTimer()
        {
            DebugTestNotification();

            _timer.Stop();

            var uri = CreateTfsUri();
            if (uri == null)
                return;

            foreach (var build in Builds)
            {
                FinalizeBuild(build);
            }

            _buildInfoProvider?.Dispose();
            _buildInfoProvider = _buildInfoProviderFactory(uri);

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            RefreshBuildInfos(); // NOTE: when there is a UI for finished builds, refreshing the first time must also get the finished builds
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            RefreshBuildInfos();
        }

        private async void RefreshBuildInfos()
        {
            // async void should be ok here. Only used from the Timer_Tick and Initialization
            // and we catch the exception
            try
            {
                var buildInfos = await _buildInfoProvider.GetCurrentBuildsAsync();
                foreach (var build in buildInfos)
                {
                    TryAdd(build);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void TryAdd(BuildInfo buildInfo)
        {
            if (_lastKnownBuilds.ContainsKey(buildInfo.Id))
                return; // we know that build already and we are connected to it.

            _lastKnownBuilds.Add(buildInfo.Id, buildInfo);
            Builds.Add(buildInfo);

            NotifyBuild(buildInfo);

            buildInfo.Connect();
            buildInfo.BuildUpdated += Build_BuildUpdated;
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

        private readonly ConcurrentDictionary<int, INotification> _notificationsByBuildId = new ConcurrentDictionary<int, INotification>();

        private static readonly BuildStatus[] _finalizableStates = new BuildStatus[]
        {
            BuildStatus.Failed, BuildStatus.PartiallySucceeded, BuildStatus.Stopped, BuildStatus.Succeeded
        };

        private void NotifyBuild(BuildInfo build)
        {
            INotification notification = _notifier.ShowBuildInfoMessage(build, _defaultOptions);
            if (_finalizableStates.Any(x => x == build.Status))
                FinalizeBuild(build);

            if (notification != null)
                _notificationsByBuildId.AddOrUpdate(build.Id, notification, (id, not) => UpdateNotificationForBuild(not, notification));
        }

        private INotification UpdateNotificationForBuild(INotification oldNotification, INotification newNotification)
        {
            oldNotification.Close();

            return newNotification;
        }

        private void FinalizeBuild(BuildInfo build)
        {
            build.Disconnect();
            build.BuildUpdated -= Build_BuildUpdated;
            _lastKnownBuilds.Remove(build.Id);
            Builds.Remove(build);
        }

        /// <summary>
        /// This is just for debugging / demo purpose and exists only in Debug builds.
        /// </summary>
        [Conditional("DEBUG")]
        private void DebugTestNotification()
        {
            var build = new BuildInfo(null)
            {
                Status = BuildStatus.Failed,
                BuildDefinitionName = "My-Build",
                BuildSummary = new Uri("http://google.com"),
                DropLocation = "C:\\",
                RequestedBy = "Michael Praschl",
                QueuedTime = DateTime.Now.AddMinutes(-23).AddSeconds(14),
                FinishTime = DateTime.Now
            };

            _notifier.ShowBuildInfoMessage(build, _defaultOptions);
        }
    }
}
