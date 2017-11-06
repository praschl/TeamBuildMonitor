using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.Providers
{
    public class BuildInfo
    {
        private IQueuedBuild _build;

        public BuildInfo(IQueuedBuild build)
        {
            _build = build;
        }

        public int Id { get; set; }
        public string TeamProject { get; set; }
        public string BuildDefinitionName { get; set; }
        public string[] ServerItems { get; set; }
        public string RequestedBy { get; set; }
        public Uri BuildSummary { get; set; }
        public string DropLocation { get; set; }
        public BuildStatus Status { get; internal set; }
        public Exception PollingException { get; internal set; }
        public DateTime QueuedTime { get; internal set; }
        public DateTime FinishTime { get; internal set; }

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
            bool changed =
                Status != _build.Build.Status
                || FinishTime != _build.Build.FinishTime;

            Status = _build.Build.Status;
            FinishTime = _build.Build.FinishTime;

            if (changed)
                OnBuildUpdated(EventArgs.Empty);
        }

        public event EventHandler<EventArgs> BuildUpdated;

        protected virtual void OnBuildUpdated(EventArgs e)
        {
            BuildUpdated?.Invoke(this, e);
        }

        //
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
