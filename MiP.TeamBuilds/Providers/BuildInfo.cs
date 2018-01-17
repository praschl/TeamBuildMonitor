using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

using Microsoft.Expression.Interactivity.Core;
using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.Providers
{
    public class BuildInfo : INotifyPropertyChanged
    {
        private IQueuedBuild _build;

        public BuildInfo(IQueuedBuild build)
        {
            _build = build;
        }

        public ICommand GotoDropLocationCommand => new ActionCommand(() =>
                                                                     {
                                                                         if (!string.IsNullOrEmpty(DropLocation))
                                                                             Process.Start(DropLocation);
                                                                     });

        public ICommand OpenBuildSummaryCommand => new ActionCommand(() =>
                                                                     {
                                                                         if (BuildSummary != null)
                                                                             Process.Start(BuildSummary.ToString());
                                                                     });

        public ICommand CancelBuildCommand => new ActionCommand(() =>
                                                                {
                                                                    if (_build.Status == QueueStatus.Postponed|| _build.Status == QueueStatus.Queued)
                                                                        _build.Cancel();
                                                                });

        public string Id { get; set; }
        public string TeamProject { get; set; }
        public string BuildDefinitionName { get; set; }
        public string[] ServerItems { get; set; }
        public string RequestedByDisplayName { get; set; }
        public string RequestedBy { get; set; }
        public string RequestedForDisplayName { get; set; }
        public string RequestedFor { get; set; }
        public Uri BuildSummary { get; set; }
        public string DropLocation { get; set; }
        public BuildStatus BuildStatus { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public Exception PollingException { get; internal set; }
        public DateTime QueuedTime { get; internal set; }
        public DateTime FinishTime { get; internal set; }

        // calculated properties
        public TimeSpan Duration => FinishTime - QueuedTime;
        public string BySort
        {
            get
            {
                if (RequestedBy == RequestedFor)
                    return RequestedByDisplayName;
                return RequestedForDisplayName;
            }
        }
        public string By
        {
            get
            {
                if (RequestedBy == RequestedFor)
                    return RequestedByDisplayName;
                return "for " + RequestedForDisplayName;
            }
        }

        public string ForSort
        {
            get
            {
                if (RequestedBy == RequestedFor)
                    return RequestedForDisplayName;
                return RequestedByDisplayName;
            }
        }
        public string For
        {
            get
            {
                if (RequestedBy == RequestedFor)
                    return RequestedForDisplayName;
                return "by " + RequestedByDisplayName;
            }
        }

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
            // Build_StatusChanged this event will be raised on another thread
            App.Current.Dispatcher.Invoke(() =>
            {
                IsChanged = false;

                QueueStatus = _build.Status;
                BuildStatus = _build.Build?.Status ?? BuildStatus.None;
                FinishTime = _build.Build?.FinishTime ?? DateTime.MinValue;

                if ((BuildStatus == BuildStatus.Stopped
                     || BuildStatus == BuildStatus.Failed
                     || BuildStatus == BuildStatus.PartiallySucceeded
                     || BuildStatus == BuildStatus.Succeeded)
                    &&
                    FinishTime == DateTime.MinValue
                )
                {
                    string message = "ERROR: Build finished with: " + BuildStatus + " but finish time is not set!!!";
                    Console.WriteLine(message);
                    throw new InvalidOperationException(message);
                }
                
                if (IsChanged)
                    OnBuildUpdated(EventArgs.Empty);
            });
        }

        public event EventHandler<EventArgs> BuildUpdated;

        protected virtual void OnBuildUpdated(EventArgs e)
        {
            BuildUpdated?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseFakeEventForDateChanges()
        {
            // This raises the PropertyChanged event for QueuedTime and FinishTime.
            // This will update the display message in the UI, where the displayed string is something like "3 minutes ago".
            // This will also trigger a filter update when the filter is something like "age:5m" (no builds older than 5 minutes).

            // Since the actual date never changes after it is set, the event would not be raised ever after, and the displayed message would never change to "4 minutes ago".
            // The same is true for the filter.

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QueuedTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishTime)));
        }
    }
}
