using System.Windows;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public interface ICompositeChild
    {
        void SetParent(NotificationBase notificationBase);
    }
}