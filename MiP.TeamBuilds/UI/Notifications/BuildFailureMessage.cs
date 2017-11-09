using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Commands;
using System;
using System.Diagnostics;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public class BuildFailureMessage : BuildMessage
    {
        public BuildFailureMessage(BuildInfo build)
            : base(build)
        {
        }

        public ICommand LinkClickCommand
        {
            get
            {
                Action<NotificationBase> click = n =>
                {
                    Process.Start(BuildInfo.BuildSummary.ToString());
                    n.Close();
                };

                return new LinkClickCommand(click, _notificationBase);
            }
        }
    }
}
