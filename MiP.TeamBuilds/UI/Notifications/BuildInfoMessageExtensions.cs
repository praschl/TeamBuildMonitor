using MiP.TeamBuilds.Providers;
using ToastNotifications;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public static class BuildInfoMessageExtensions
    {
        public static INotification ShowBuildInfoMessage(this Notifier notifier, BuildInfo buildInfo, MessageOptions displayOptions = null)
        {
            var message = new BuildInfoMessage(buildInfo, displayOptions);
            notifier.Notify<BuildInfoMessage>(() => message);
            return message;
        }
    }
}
