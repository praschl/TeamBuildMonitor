using MiP.TeamBuilds.Providers;
using System;
using System.Diagnostics;
using System.Windows.Input;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Main
{
    public class BuildSuccessMessage : BuildMessage
    {
        public BuildSuccessMessage(BuildInfo build)
            : base(build)
        {
        }

        public ICommand LinkClickCommand
        {
            get
            {
                Action<NotificationBase> click = n =>
                {
                    Process.Start(BuildInfo.DropLocation);
                    n.Close();
                };

                return new LinkClickCommand(click, _notificationBase);
            }
        }
    }
}
