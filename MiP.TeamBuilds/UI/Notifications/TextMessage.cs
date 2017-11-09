using MiP.TeamBuilds.UI.CompositeNotifications;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class TextMessage : NotificationContent
    {
        public TextMessage(string title, string message)
            : base(title)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
