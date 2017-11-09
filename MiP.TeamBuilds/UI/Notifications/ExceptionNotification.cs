using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System;
using System.Windows;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class ExceptionMessage : NotificationContent
    {
        public ExceptionMessage(string title, Exception ex, Notifier notifier)
            : base(title)
        {
            Message = ex.Message;
            _exception = ex;
            _notifier = notifier;
        }

        public string Message { get; set; }

        private readonly Exception _exception;
        private readonly Notifier _notifier;

        public string LinkText => "Copy to clipboard...";

        public ICommand LinkClickCommand
        {
            get
            {
                Action<NotificationBase> linkClickAction = n =>
                {
                    Clipboard.SetText(_exception.ToString());

                    var content = new TextMessage("Done", "Exception copied to clipboard.");

                    _notifier.ShowInformation(content);
                    n.Close();
                };

                return new LinkClickCommand(linkClickAction, _notificationBase);
            }
        }
    }
}
