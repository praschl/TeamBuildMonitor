﻿using System;
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

namespace MiP.TeamBuilds.UI.Notifications
{
    // TODO: move notification system out of KnownBuildsViewModel 

    [AddINotifyPropertyChangedInterface]
    public class KnownBuildsViewModel
    {
        private Owned<IBuildInfoProvider> _buildInfoProvider;

        private readonly Notifier _notifier;

        private readonly Dictionary<string, BuildInfo> _lastKnownBuilds = new Dictionary<string, BuildInfo>();
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

        public ICommand ShowSettingsCommand { get; }

        public void RefreshTfsProvider()
        {
            var uri = CreateTfsUri();
            if (uri == null)
                return;

            foreach (var build in Builds.ToArray())
            {
                FinalizeBuild(build);
            }

            _buildInfoProvider?.Dispose();
            _buildInfoProvider = _buildInfoProviderFactory.GetProvider(uri);

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

        public async void RefreshBuildInfos()
        {
            // async-void should be ok here. Only used from the Timer_Tick and Initialization
            // and we catch the exception
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
            if (_finalizableStates.Any(x => x == build.Status))
                FinalizeBuild(build);
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

    }
}
