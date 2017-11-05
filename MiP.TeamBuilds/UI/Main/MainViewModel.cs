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
                notificationLifetime: TimeSpan.FromSeconds(6),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

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
                ShowCloseButton = false,
                NotificationClickAction = n =>
                {
                    n.Close();

                    ShowSettingsCommand.Execute(null);
                }
            };

            var message = new TextWithLinkAction
            {
                Message = "Uri to TFS has not been set yet.",
                LinkText = "Click here to set it"
            };

            _notifier.ShowInformation("Setup", message, displayOptions);
        }

        private void ShowException(Exception ex)
        {
            var message = ex.Message + Environment.NewLine + "Click to copy exception to clipboard.";

            var errorDisplayOptions = new MessageOptions
            {
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = true,
                NotificationClickAction = n =>
                {
                    Clipboard.SetText(ex.ToString());
                    _notifier.ShowInformation("Exception copied to clipboard.", null);
                    n.Close();
                }
            };

            _notifier.ShowError(message, errorDisplayOptions);
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
                _notifier.ShowError(ex.Message, null);
            }
        }

        private void NotifyBuild(BuildInfo build)
        {
            var message = $"Build {build.Status}: {build.BuildDefinitionName}";

            switch (build.Status)
            {
                case BuildStatus.Failed:
                    _notifier.ShowError(message, _defaultOptions); // TODO: show link to build page of tfs
                    FinalizeBuild(build);
                    break;

                case BuildStatus.PartiallySucceeded:
                case BuildStatus.Stopped:
                    _notifier.ShowWarning(message, _defaultOptions); // TODO: show link to build page of tfs
                    FinalizeBuild(build);
                    break;

                case BuildStatus.Succeeded:
                    _notifier.ShowSuccess(message, _defaultOptions); // TODO: link to open drop folder
                    FinalizeBuild(build);
                    break;

                case BuildStatus.None:
                case BuildStatus.NotStarted:
                case BuildStatus.InProgress:
                    _notifier.ShowInformation(message, _defaultOptions);
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


    public class TextWithLinkAction
    {
        public string Message { get; set; }
        public string LinkText { get; set; }
    }
}
