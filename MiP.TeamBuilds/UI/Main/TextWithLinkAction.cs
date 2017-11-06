using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Main
{
    public class TextWithLinkNotification : ICompositeChild
    {
        private readonly Action<NotificationBase> _linkClickAction;
        private NotificationBase _notificationBase;

        public TextWithLinkNotification(string message, string link, Action<NotificationBase> linkClickAction)
        {
            Message = message;
            LinkText = link;
            _linkClickAction = linkClickAction;
        }

        public string Message { get; set; }

        public string LinkText { get; set; }

        public ICommand LinkClickCommand
        {
            get
            {
                return new LinkClickCommand(_linkClickAction, _notificationBase);
            }
        }

        public void SetParent(NotificationBase notificationBase)
        {
            _notificationBase = notificationBase;
        }
    }
}
