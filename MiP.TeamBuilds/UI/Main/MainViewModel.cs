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

        public void RestartTimer()
        {
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

            var message = new TextWithLinkNotification("Uri to TFS has not been set yet.", "Go to settings...",
                n =>
                {
                    n.Close();
                    ShowSettingsCommand.Execute(null);
                });

            _notifier.ShowInformation("Setup", message, displayOptions);
        }

        private void ShowException(Exception ex)
        {
            var errorDisplayOptions = new MessageOptions
            {
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = true
            };

            var message = new TextWithLinkNotification(ex.Message, "Copy to clipboard...",
                n =>
                {
                    Clipboard.SetText(ex.ToString());
                    // TODO: make better notification type and xaml for this one
                    _notifier.ShowInformation("Done", new TextWithLinkNotification("Exception copied to clipboard.", null, null), null);
                    n.Close();
                });

            _notifier.ShowError("Exception", message, errorDisplayOptions);
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
                    build.PropertyChanged += Build_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        private void NotifyBuild(BuildInfo build)
        {
            var title = build.BuildDefinitionName;
            var message = $"Build {build.Status}";

            object createSummaryLink() => new TextWithLinkNotification(message, "Open build...", n =>
             {
                 Process.Start(build.BuildSummary.ToString());
                 n.Close();
             });

            // TODO: create a notification which takes an instance of build. use the build properties in the template
            // (display requested by)
            switch (build.Status)
            {
                case BuildStatus.Failed:

                    _notifier.ShowError(title, createSummaryLink(), _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.PartiallySucceeded:
                case BuildStatus.Stopped:

                    _notifier.ShowWarning(title, createSummaryLink(), _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.Succeeded:
                    var contentSuccess = new TextWithLinkNotification(message, "Open drop location...", n =>
                    {
                        Process.Start(build.DropLocation);
                        n.Close();
                    });
                    // TODO: better check of drop location, and do not unnecessarily instantiate contentSuccess
                    if (string.IsNullOrEmpty(build.DropLocation))
                        contentSuccess = new TextWithLinkNotification(message, null, null);

                    _notifier.ShowSuccess(title, contentSuccess, _defaultOptions);
                    FinalizeBuild(build);
                    break;

                case BuildStatus.None:
                case BuildStatus.NotStarted:
                case BuildStatus.InProgress:
                    var contentInProgress = new TextWithLinkNotification(message, null, null); // TODO: simple text notification
                    _notifier.ShowInformation(title, contentInProgress, _defaultOptions);
                    break;
            }
        }

        private void FinalizeBuild(BuildInfo build)
        {
            build.Disconnect();
            build.PropertyChanged -= Build_PropertyChanged;
            _lastKnownBuilds.Remove(build.Id);
        }

        private void Build_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BuildInfo build = (BuildInfo)sender;
                NotifyBuild(build);

                if (build.PollingException != null)
                {
                    ShowException(build.PollingException);
                }
            });
        }

        public ICommand ShowSettingsCommand { get => _showSettingsCommandFactory(); }

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
