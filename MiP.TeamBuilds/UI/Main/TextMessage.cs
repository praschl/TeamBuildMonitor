using MiP.TeamBuilds.UI.Notifications;

namespace MiP.TeamBuilds.UI.Main
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
