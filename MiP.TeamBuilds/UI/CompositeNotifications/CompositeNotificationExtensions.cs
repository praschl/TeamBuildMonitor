using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.CompositeNotifications
{
    public static class CompositeNotificationExtensions
    {
        public static INotification ShowError(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            var compositeMessage = new CompositeMessage(NotificationStyle.Error, content, displayOptions);
            notifier.Notify<CompositeMessage>(() => compositeMessage);
            return compositeMessage;
        }

        public static INotification ShowWarning(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            var compositeMessage = new CompositeMessage(NotificationStyle.Warning, content, displayOptions);
            notifier.Notify<CompositeMessage>(() => compositeMessage);
            return compositeMessage;
        }

        public static INotification ShowSuccess(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            var compositeMessage = new CompositeMessage(NotificationStyle.Success, content, displayOptions);
            notifier.Notify<CompositeMessage>(() => compositeMessage);
            return compositeMessage;
        }

        public static INotification ShowInformation(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            var compositeMessage = new CompositeMessage(NotificationStyle.Information, content, displayOptions);
            notifier.Notify<CompositeMessage>(() => compositeMessage);
            return compositeMessage;
        }
    }
}