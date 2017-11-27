using System;
using Microsoft.TeamFoundation.Build.Client;
using PropertyChanged;

namespace MiP.TeamBuilds.Providers
{
    [AddINotifyPropertyChangedInterface]
    public class BuildInfo
    {
        private IQueuedBuild _build;

        public BuildInfo(IQueuedBuild build)
        {
            _build = build;
        }

        public string Id { get; set; }
        public string TeamProject { get; set; }
        public string BuildDefinitionName { get; set; }
        public string[] ServerItems { get; set; }
        public string RequestedBy { get; set; }
        public Uri BuildSummary { get; set; }
        public string DropLocation { get; set; }
        public BuildStatus BuildStatus { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public Exception PollingException { get; internal set; }
        public DateTime QueuedTime { get; internal set; }
        public DateTime FinishTime { get; internal set; }

        public bool IsChanged { get; set; }

        public void Connect()
        {
            _build.Connect();
            _build.StatusChanged += Build_StatusChanged;
        }

        public void Disconnect()
        {
            _build.StatusChanged -= Build_StatusChanged;
            _build.Disconnect();
        }

        private void Build_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            IsChanged = false;

            QueueStatus = _build.Status;
            BuildStatus = _build.Build?.Status ?? BuildStatus.None;
            FinishTime = _build.Build?.FinishTime ?? DateTime.MinValue;

            if (IsChanged)
                OnBuildUpdated(EventArgs.Empty);
        }

        public event EventHandler<EventArgs> BuildUpdated;

        protected virtual void OnBuildUpdated(EventArgs e)
        {
            BuildUpdated?.Invoke(this, e);
        }
    }
}
