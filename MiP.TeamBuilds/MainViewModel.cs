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

namespace MiP.TeamBuilds
{
    public class MainViewModel : INotifyPropertyChanged, IRestartTimer
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TfsBuildHelper _tfsBuildHelper;
        private Dictionary<int, BuildInfo> _lastKnownBuilds = new Dictionary<int, BuildInfo>();

        private readonly Notifier _buildNotifier = new Notifier(cfg =>
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

        private readonly Notifier _errorNotifier = new Notifier(cfg =>
        {
            var offset = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 0, offset);

            cfg.LifetimeSupervisor = new CountBasedLifetimeSupervisor(maximumNotificationCount: MaximumNotificationCount.FromCount(3));

            cfg.Dispatcher = Application.Current.Dispatcher;

            cfg.DisplayOptions.TopMost = true;
        });

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

            MessageOptions displayOptions = new MessageOptions
            {
                ShowCloseButton = true,
                NotificationClickAction = n =>
                {
                    Clipboard.SetText(ex.ToString());
                    _buildNotifier.ShowInformation("Exception copied to clipboard.");
                    n.Close();
                }
            };

            _errorNotifier.ShowError(message, displayOptions);
        }
        
        private void ShowTfsUrlNotSet()
        {
            var message = "Uri to TFS has not been set yet. Click here to set it!";

            MessageOptions displayOptions = new MessageOptions
            {
                ShowCloseButton = false,
                NotificationClickAction = n =>
                {
                    n.Close();

                    SettingsWindow settings = new SettingsWindow(this); // TODO: resolve settings window...
                    settings.ShowDialog();
                    settings.Close();
                }
            };

            _errorNotifier.ShowInformation(message, displayOptions);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var currentBuilds = _tfsBuildHelper.GetCurrentBuilds().ToList();

                // TODO: connect to all builds not finished and display changes to state.

                foreach (var build in currentBuilds)
                {
                    if (!build.Finished)
                        ShowBuildStarted(build);
                    else
                        ShowBuildFinished(build);
                }
            }
            catch (Exception ex)
            {
                _buildNotifier.ShowError(ex.Message);
            }
        }

        private void ShowBuildFinished(BuildInfo build)
        {
            if (!_lastKnownBuilds.ContainsKey(build.Id))
                return; // was already shown as finished, or at least, never shown as started (when application started after build finished).

            var message = $"Build {build.Status}: {build.BuildDefinitionName}";
            
            switch (build.Status)
            {
                case BuildStatus.Failed:
                    _buildNotifier.ShowError(message, _defaultOptions); // TODO: show link to build page of tfs
                    break;

                case BuildStatus.PartiallySucceeded:
                case BuildStatus.Stopped:
                    _buildNotifier.ShowWarning(message, _defaultOptions); // TODO: show link to build page of tfs
                    break;

                case BuildStatus.Succeeded:
                    _buildNotifier.ShowSuccess(message, _defaultOptions); // TODO: link to open drop folder
                    break;
            }

            _lastKnownBuilds.Remove(build.Id);
        }

        private void ShowBuildStarted(BuildInfo build)
        {
            if (_lastKnownBuilds.ContainsKey(build.Id))
                return; // was already shown as started.

            var message = $"Build {build.Status}: {build.BuildDefinitionName}";
            
            _buildNotifier.ShowInformation(message, _defaultOptions);

            _lastKnownBuilds.Add(build.Id, build);
        }

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
