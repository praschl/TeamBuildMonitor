using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using Microsoft.TeamFoundation.Build.Client;

using MiP.TeamBuilds.Providers;

using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.UI.Notifications;
using System.Windows.Threading;
using System.Windows.Input;
using MiP.TeamBuilds.UI.Commands;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MiP.TeamBuilds.UI.Main
{
    public interface IRefreshBuildsTimer
    {
        void RestartTimer();
    }
}
