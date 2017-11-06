using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Main
{
    public class ExceptionNotification : ICompositeChild
    {
        private NotificationBase _notificationBase;

        public ExceptionNotification(Exception ex, Notifier notifier)
        {
            Message = ex.Message;
            _exception = ex;
            _notifier = notifier;
        }

        public string Message { get; set; }

        private readonly Exception _exception;
        private readonly Notifier _notifier;

        public string LinkText { get => "Copy to clipboard..."; }

        public ICommand LinkClickCommand
        {
            get
            {
                Action<NotificationBase> linkClickAction = n =>
                {
                    Clipboard.SetText(_exception.ToString());
                    _notifier.ShowInformation("Done", new TextNotification("Exception copied to clipboard."));
                    n.Close();
                };

                return new LinkClickCommand(linkClickAction, _notificationBase);
            }
        }

        public void SetParent(NotificationBase notificationBase)
        {
            _notificationBase = notificationBase;
        }
    }
}
