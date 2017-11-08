using System;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Commands
{
    public class LinkClickCommand : ICommand
    {
        private readonly Action<NotificationBase> _linkClickAction;
        private readonly NotificationBase _notificationBase;

        public LinkClickCommand(Action<NotificationBase> linkClickAction, NotificationBase notificationBase)
        {
            _linkClickAction = linkClickAction;
            _notificationBase = notificationBase;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _linkClickAction?.Invoke(_notificationBase);
        }
    }
}
