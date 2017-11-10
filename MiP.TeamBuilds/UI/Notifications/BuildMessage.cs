using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.CompositeNotifications;
namespace MiP.TeamBuilds.UI.Notifications
{
    public class BuildMessage : NotificationContent
    {
        public BuildMessage(BuildInfo build)
            : base($"{build.BuildDefinitionName} (by {build.RequestedBy})")
        {
            BuildInfo = build;
        }

        protected BuildInfo BuildInfo { get; }

        public string BuildState => BuildInfo.Status.ToString();
    }
}
