using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public static class CompositeNotificationExtensions
    {
        public static void ShowError(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Error, content, displayOptions));
        }

        public static void ShowWarning(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Warning, content, displayOptions));
        }

        public static void ShowSuccess(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Success, content, displayOptions));
        }

        public static void ShowInformation(this Notifier notifier, NotificationContent content, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Information, content, displayOptions));
        }
    }
}