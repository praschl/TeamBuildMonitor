using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;

using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.UI.Notifications;
using System.Windows.Threading;
using System.Windows.Input;
using MiP.TeamBuilds.UI.Commands;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MiP.TeamBuilds.UI.Main
{
    public class KnownBuildsModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Dictionary<int, BuildInfo> _lastKnownBuilds = new Dictionary<int, BuildInfo>();
        private readonly ObservableCollection<BuildInfo> _buildsList = new ObservableCollection<BuildInfo>();
        private readonly Notifier _notifier;
        private BuildInfoProvider _buildInfoProvider;

        private readonly MessageOptions _defaultOptions = new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true,
            ShowCloseButton = false,
            NotificationClickAction = n => n.Close()
        };
        private readonly Func<ShowSettingsCommand> _showSettingsCommandFactory;

        public KnownBuildsModel(Notifier notifier, Func<ShowSettingsCommand> showSettingsCommandFactory)
        {
            _notifier = notifier;
            _showSettingsCommandFactory = showSettingsCommandFactory;

            CurrentBuildsView = CollectionViewSource.GetDefaultView(_buildsList);
        }

        public ICollectionView CurrentBuildsView { get; }

        public ICommand ShowSettingsCommand => _showSettingsCommandFactory();

        public void RestartTimer()
        {
            DebugTestNotification();

            _timer.Stop();

            var uri = CreateTfsUri();
            if (uri == null)
                return;

            _buildInfoProvider?.Dispose();
            _buildInfoProvider = new BuildInfoProvider(uri);

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
                var buildInfos = await _buildInfoProvider.GetCurrentBuildsAsync();

                foreach (var build in buildInfos)
                {
                    TryAdd(build);
                }
                // TODO: check if its required to remove any build which was not retrieved anymore, so they dont hang around forever...
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
            _buildsList.Add(buildInfo);

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
            _buildsList.Remove(build);
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

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
