using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            // TODO: remember notifications by BuildId. close existing notifications of a build before adding a new one.

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

        private void NotifyBuild(BuildInfo build)
        {
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
            Builds.Remove(build);
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
    }
}
