using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Commands;
using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;
using System.Diagnostics;
using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class BuildInfoMessage : NotificationBase
    {
        private BuildInfo _buildInfo;

        public QueueStatus QueueStatus => _buildInfo.QueueStatus;
        public BuildStatus BuildStatus => _buildInfo.BuildStatus;
        public string BuildDefinitionName => _buildInfo.BuildDefinitionName;
        public string RequestedByDisplayName => _buildInfo.RequestedByDisplayName;

        public ICommand DropLocationCommand => new LinkClickCommand(n =>
        {
            Process.Start(_buildInfo.DropLocation);
            n.Close();
        }, this);

        public ICommand BuildSummaryCommand => new LinkClickCommand(n =>
        {
            Process.Start(_buildInfo.BuildSummary.ToString());
            n.Close();
        }, this);

        public BuildInfoMessage(BuildInfo buildInfo, MessageOptions options = null)
        {
            Options = options ?? new MessageOptions();
            _buildInfo = buildInfo;
        }

        public NotificationDisplayPart _displayPart;
        public MessageOptions Options;

        public override NotificationDisplayPart DisplayPart => _displayPart ?? (_displayPart = Configure());

        private BuildInfoDisplayPart Configure()
        {
            BuildInfoDisplayPart displayPart = CreateDisplayPart();

            displayPart.Unloaded += OnUnloaded;
            displayPart.MouseLeftButtonDown += OnLeftMouseDown;

            UpdateDisplayOptions(displayPart, Options);
            return displayPart;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _displayPart.MouseLeftButtonDown -= OnLeftMouseDown;
            _displayPart.Unloaded -= OnUnloaded;
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            Options.NotificationClickAction?.Invoke(this);
        }

        private BuildInfoDisplayPart CreateDisplayPart()
        {
            return new BuildInfoDisplayPart(this);
        }

        private void UpdateDisplayOptions(BuildInfoDisplayPart displayPart, MessageOptions options)
        {
            if (options.FontSize != null)
                displayPart.Text.FontSize = options.FontSize.Value;

            displayPart.CloseButton.Visibility = options.ShowCloseButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
