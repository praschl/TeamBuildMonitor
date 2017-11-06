using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public abstract class NotificationContent
    {
        protected NotificationBase _notificationBase;

        public string Title { get; }

        public NotificationContent(string title)
        {
            Title = title;
        }

        public void Initialize(NotificationBase notificationBase)
        {
            _notificationBase = notificationBase;
        }
    }
}