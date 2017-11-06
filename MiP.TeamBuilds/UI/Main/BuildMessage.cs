﻿using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System;

namespace MiP.TeamBuilds.UI.Main
{
    public class BuildMessage : NotificationContent
    {
        public BuildMessage(BuildInfo build)
            : base(build.BuildDefinitionName)
        {
            BuildInfo = build;
        }

        protected BuildInfo BuildInfo { get; }

        public string BuildState => BuildInfo.Status.ToString();

        public string RequestedBy => BuildInfo.RequestedBy;

        public TimeSpan QueuedTime => BuildInfo.QueuedTime.TimeOfDay;

        public TimeSpan BuildTime => BuildInfo.FinishTime - BuildInfo.QueuedTime;
    }
}
