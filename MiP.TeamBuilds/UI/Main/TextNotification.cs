using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Main
{

    public class TextNotification : ICompositeChild
    {
        private NotificationBase _notificationBase;

        public TextNotification(string message)
        {
            Message = message;
        }

        public string Message { get; set; }

        public void SetParent(NotificationBase notificationBase)
        {
            _notificationBase = notificationBase;
        }
    }
}
