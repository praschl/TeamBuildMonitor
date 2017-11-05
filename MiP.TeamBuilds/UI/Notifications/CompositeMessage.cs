using System.Windows;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class CompositeMessage : NotificationBase
    {
        public CompositeMessage(string title, object content) : this(title, content, new MessageOptions())
        {
        }

        public CompositeMessage(string title, object content, MessageOptions options)
        {
            Title = title;
            Content = content;

            Options = options ?? new MessageOptions();
        }

        protected NotificationDisplayPart _displayPart;
        internal readonly MessageOptions Options;

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

    public static class CompositeNotificationExtensions
    {
        public static void ShowComposite(this Notifier notifier, string title, object inner)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(title, inner));
        }

        public static void ShowComposite(this Notifier notifier, string title, object inner, MessageOptions displayOptions)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(title, inner, displayOptions));
        }
    }

    public class Inner
    {
        public string Msg { get; set; }
    }
}