using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.CompositeNotifications
{
    public class CompositeMessage : NotificationBase
    {
        public CompositeMessage(NotificationStyle style, NotificationContent content, MessageOptions options = null)
        {
            Content = content;
            Style = style;

            Options = options ?? new MessageOptions();

            content.Initialize(this);
        }

        public NotificationDisplayPart _displayPart;
        public MessageOptions Options;

        public NotificationStyle Style { get; }
        public object Content { get; }

        public override NotificationDisplayPart DisplayPart => _displayPart ?? (_displayPart = Configure());

        private CompositeDisplayPart Configure()
        {
            CompositeDisplayPart displayPart = CreateDisplayPart();

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

        private CompositeDisplayPart CreateDisplayPart()
        {
            return new CompositeDisplayPart(this);
        }

        private void UpdateDisplayOptions(CompositeDisplayPart displayPart, MessageOptions options)
        {
            if (options.FontSize != null)
                displayPart.Text.FontSize = options.FontSize.Value;

            displayPart.CloseButton.Visibility = options.ShowCloseButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}