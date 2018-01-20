using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;

using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System.Windows.Input;
using MiP.TeamBuilds.UI.Commands;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using Autofac.Features.OwnedInstances;
using PropertyChanged;
using System.Threading.Tasks;
using MiP.TeamBuilds.Configuration;

namespace MiP.TeamBuilds.UI.Notifications
{
    [AddINotifyPropertyChangedInterface]
    public class KnownBuildsViewModel
    {
        private Owned<IBuildInfoProvider> _buildInfoProvider;

        private readonly Notifier _notifier;

        private readonly Dictionary<string, BuildInfo> _buildsById = new Dictionary<string, BuildInfo>();
        private readonly ConcurrentDictionary<string, INotification> _notificationsByBuildId = new ConcurrentDictionary<string, INotification>();

        private readonly MessageOptions _defaultOptions = new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true,
            ShowCloseButton = false,
            NotificationClickAction = n => n.Close()
        };

        private readonly BuildInfoProviderFactory _buildInfoProviderFactory;

        public KnownBuildsViewModel(Notifier notifier, ShowSettingsCommand showSettingsCommand, BuildInfoProviderFactory buildInfoProviderFactory)
        {
            _notifier = notifier;
            ShowSettingsCommand = showSettingsCommand;
            _buildInfoProviderFactory = buildInfoProviderFactory;
        }

        public ObservableCollection<BuildInfo> Builds { get; } = new ObservableCollection<BuildInfo>();
        public bool NotificationsEnabled { get; set; } = true;

        public bool IsBusy { get; set; }
        public event EventHandler<EventArgs> IsBusyChanged;
        private void OnIsBusyChanged() // called by IsBusy.set (modified by Fody.PropertyChanged)
        {
            IsBusyChanged?.Invoke(this, EventArgs.Empty);
        }

        public ICommand ShowSettingsCommand { get; }

        public async void RebuildTfsProvider()
        {
            try
            {
                IsBusy = true;

                var uri = CreateTfsUri();
                if (uri == null)
                    return;

                foreach (var build in Builds.ToArray())
                {
                    FinalizeBuild(build);
                }
                Builds.Clear();
                _buildsById.Clear(); // should already be clear by calling FinalizeBuild(build) for all buildinfos
                _buildInfoProvider?.Dispose();
                _buildInfoProvider = _buildInfoProviderFactory.GetProvider(uri);

                var finished = RefreshFinishedBuildInfosAsync();
                var current = RefreshBuildInfosAsync();
                await Task.WhenAll(finished, current);
            }
            catch(Exception ex)
            {
                ShowException(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Uri CreateTfsUri()
        {
            if (string.IsNullOrEmpty(JsonConfig.Instance.TfsUrl))
            {
                ShowTfsUrlNotSet();
                return null;
            }

            try
            {
                return new Uri(JsonConfig.Instance.TfsUrl);
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

        public async Task RefreshFinishedBuildInfosAsync()
        {
            try
            {
                var buildInfos = (await _buildInfoProvider.Value.GetRecentlyFinishedBuildsAsync()).ToArray();

                foreach (var build in buildInfos)
                {
                    Builds.Add(build);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        public async Task RefreshBuildInfosAsync()
        {
            try
            {
                var buildInfos = await _buildInfoProvider.Value.GetCurrentBuildsAsync();
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
            if (_buildsById.ContainsKey(buildInfo.Id))
                return; // we know that build already and we are connected to it.

            _buildsById.Add(buildInfo.Id, buildInfo);
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

        private static readonly BuildStatus[] _finalizableStates = new BuildStatus[]
        {
            BuildStatus.Failed, BuildStatus.PartiallySucceeded, BuildStatus.Stopped, BuildStatus.Succeeded
        };

        private void NotifyBuild(BuildInfo build)
        {
            if (NotificationsEnabled)
            {
                INotification notification = _notifier.ShowBuildInfoMessage(build, _defaultOptions);

                if (notification != null)
                    _notificationsByBuildId.AddOrUpdate(build.Id, notification, (id, not) => UpdateNotificationForBuild(not, notification));
            }
            if (_finalizableStates.Any(x => x == build.BuildStatus))
                FinalizeBuild(build);
        }

        private INotification UpdateNotificationForBuild(INotification oldNotification, INotification newNotification)
        {
            // Closing notifications which have not yet been shown (because there are too many) will cause a NullReferenceException.
            // However, the Id of such notifications is still 0, so we can check that.
            // in such cases, the "InProgress" status will not remove the notification for "NotStarted" and both are displayed.
            if (oldNotification.Id != 0)
                oldNotification.Close();

            return newNotification;
        }

        private void FinalizeBuild(BuildInfo build)
        {
            build.Disconnect();
            build.BuildUpdated -= Build_BuildUpdated;
            _buildsById.Remove(build.Id);
            //Builds.Remove(build);
        }
    }
}
