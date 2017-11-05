using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class CompositeMessage : NotificationBase
    {
        public CompositeMessage(NotificationStyle style, string title, object content) 
            : this(style, title, content, new MessageOptions())
        {
        }

        public CompositeMessage(NotificationStyle style, string title, object content, MessageOptions options)
        {
            Title = title;
            Content = content;
            Style = style;

            Options = options ?? new MessageOptions();
        }

        public NotificationDisplayPart _displayPart;
        public MessageOptions Options;

        public NotificationStyle Style { get; }
        public string Title { get; }
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

        protected CompositeDisplayPart CreateDisplayPart()
        {
            return new CompositeDisplayPart(this);
        }

        protected void UpdateDisplayOptions(CompositeDisplayPart displayPart, MessageOptions options)
        {
            if (options.FontSize != null)
                displayPart.Text.FontSize = options.FontSize.Value;

            displayPart.CloseButton.Visibility = options.ShowCloseButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public enum NotificationStyle
    {
        Information,
        Success,
        Warning,
        Error
    }

    // TODO: just here while developing
    public class Inner
    {
        public string Msg { get; set; }
    }
}