using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public static class CompositeNotificationExtensions
    {
        public static void ShowError(this Notifier notifier, string title, object inner, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Error, title, inner, displayOptions));
        }

        public static void ShowWarning(this Notifier notifier, string title, object inner, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Warning, title, inner, displayOptions));
        }

        public static void ShowSuccess(this Notifier notifier, string title, object inner, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Success, title, inner, displayOptions));
        }

        public static void ShowInformation(this Notifier notifier, string title, object inner, MessageOptions displayOptions = null)
        {
            notifier.Notify<CompositeMessage>(() => new CompositeMessage(NotificationStyle.Information, title, inner, displayOptions));
        }
    }
}