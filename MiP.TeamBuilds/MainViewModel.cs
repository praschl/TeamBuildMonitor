using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using Microsoft.TeamFoundation.Build.Client;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MiP.TeamBuilds.Properties;
using ToastNotifications.Core;
using System.Windows.Input;

namespace MiP.TeamBuilds
{
    public class MainViewModel : INotifyPropertyChanged, IRestartTimer
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TfsBuildHelper _tfsBuildHelper;
        private Dictionary<int, BuildInfo> _lastKnownBuilds = new Dictionary<int, BuildInfo>();

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

        public MainViewModel(ShowSettingsCommand showSettingsCommand)
        {
            ShowSettingsCommand = showSettingsCommand;
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
            if (string.IsNullOrEmpty(Settings.Default.TfsUrl))
            {
                ShowTfsUrlNotSet();
                return null;
            }

            try
            {
                return new Uri(Settings.Default.TfsUrl);
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return null;
            }
        }
        
        private void ShowException(Exception ex)
        {            
            var message = ex.Message + Environment.NewLine + "Click to copy exception to clipboard.";

            MessageOptions errorDisplayOptions = new MessageOptions
            {
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = true,
                ShowCloseButton = true,
                NotificationClickAction = n =>
                {
                    Clipboard.SetText(ex.ToString());
                    _notifier.ShowInformation("Exception copied to clipboard.");
                    n.Close();
                }
            };

            _notifier.ShowError(message, errorDisplayOptions);
        }
        
        private void ShowTfsUrlNotSet()
        {
            var message = "Uri to TFS has not been set yet. Click here to set it!";

            MessageOptions displayOptions = new MessageOptions
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

            _notifier.ShowInformation(message, displayOptions);
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var currentBuilds = _tfsBuildHelper.GetCurrentBuilds().ToList();

                foreach (var build in currentBuilds)
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
                _notifier.ShowError(ex.Message);
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

        public ICommand ShowSettingsCommand { get; }

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
