using MiP.TeamBuilds.UI.Notifications;

namespace MiP.TeamBuilds.UI.Main
{
    public class TextNotification : NotificationContent
    {
        public TextNotification(string title, string message)
            : base(title)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
