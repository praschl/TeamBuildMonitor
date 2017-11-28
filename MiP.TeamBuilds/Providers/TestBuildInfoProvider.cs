using Microsoft.TeamFoundation.Build.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MiP.TeamBuilds.Providers
{
    /// <summary>
    /// This is a test provider, which simulates a very busy TFS.
    /// It returns faked BuildInfos from GetCurrentBuildsAsync, and also changes their state.
    /// This makes the application look like it is connected to a very busy TFS.
    /// </summary>
    public sealed class TestBuildInfoProvider : IBuildInfoProvider, IDisposable
    {
        public Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync()
        {
            // here we return the faked builds by getting them from the bag and removing it from there.
            var result = new List<BuildInfo>();

            while (!_nextResults.IsEmpty)
            {
                _nextResults.TryTake(out BuildInfo buildInfo);
                result.Add(buildInfo);
            }

            return Task.FromResult((IEnumerable<BuildInfo>)result);
        }

        public Task<IEnumerable<BuildInfo>> GetRecentlyFinishedBuildsAsync()
        {
            var results = new List<BuildInfo>();
            for(int i=0;i<10;i++)
            {
                results.Add(CreateRandomFinishedBuildInfo());
            }
            return Task.FromResult((IEnumerable<BuildInfo>)results);
        }

        private DispatcherTimer _timer;

        public void Initialize(Uri uri)
        {
            // initializing starts a timer which generates faked build infos.
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private ConcurrentBag<BuildInfo> _nextResults = new ConcurrentBag<BuildInfo>();

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var runningBuilds = _running.ToArray();

            // we go through the running builds and check for

            foreach (var pair in runningBuilds)
            {
                var testBuild = pair.Key;

                // 1. their status to change it from NotStarted to InProgress
                if (testBuild.Build.Status == BuildStatus.NotStarted)
                {
                    testBuild.Build.Status = BuildStatus.InProgress;
                    testBuild.OnStatusChanged(new StatusChangedEventArgs(true));
                }

                // 2. if the build should stop
                if (pair.Value < now)
                {
                    ((TestBuildDetail)testBuild.Build).FinishTime = pair.Value;
                    testBuild.Build.Status = Pick(_finishedStatus);
                    _running.Remove(pair.Key);
                    testBuild.OnStatusChanged(new StatusChangedEventArgs(true));
                }
            }

            if (_running.Count > 10)
                return;

            _nextResults.Add(CreateRandomBuildInfo());
        }

        private readonly Random _random = new Random();

        private Dictionary<TestQueuedBuild, DateTime> _running = new Dictionary<TestQueuedBuild, DateTime>();

        private BuildInfo CreateRandomFinishedBuildInfo()
        {
            var build = new TestQueuedBuild();
            build.Build = new TestBuildDetail();

            var result = new BuildInfo(build)
            {
                Id = "TestBuildInfoProvider_" + Guid.NewGuid(),
                TeamProject = Pick(_teamProjects),
                BuildDefinitionName = Pick(_buildDefinitionNames),
                BuildSummary = Pick(_buildSummaries),
                DropLocation = Pick(_dropLocations),
                RequestedByDisplayName = Pick(_users),
                BuildStatus = Pick(_finishedStatus),
                QueueStatus = QueueStatus.Completed,
                QueuedTime = DateTime.Now.AddMinutes(-_random.Next(200, 600)),
                FinishTime = DateTime.Now.AddMinutes(-_random.Next(0, 200)),
            };

            return result;
        }

        private BuildInfo CreateRandomBuildInfo()
        {
            // lets create a random build.
            // unfortunately we need these test classes, too.
            var build = new TestQueuedBuild();
            build.Build = new TestBuildDetail();

            // some random values for display.
            var result = new BuildInfo(build)
            {
                Id = "TestBuildInfoProvider_" + Guid.NewGuid(),
                TeamProject = Pick(_teamProjects),
                BuildDefinitionName = Pick(_buildDefinitionNames),
                BuildSummary = Pick(_buildSummaries),
                DropLocation = Pick(_dropLocations),
                QueuedTime = DateTime.Now,
                RequestedByDisplayName = Pick(_users),
                BuildStatus = Pick(_status),
                QueueStatus = Pick(_queueStatus)
            };

            // we also set the status of the faked TFS build.
            build.Build.Status = result.BuildStatus;
            build.Status = Pick(_queueStatus);

            // the build will finish after 15-30 seconds.
            var plannedFinish = DateTime.Now.AddSeconds(_random.Next(15, 30));

            // and will be checked for status and finish on the next Timer_Tick
            _running.Add(build, plannedFinish);

            return result;
        }

        private T Pick<T>(IReadOnlyList<T> items)
        {
            int index = _random.Next(0, items.Count);
            return items[index];
        }

        private readonly string[] _teamProjects = { "Project 1", "Project 2", "Project 3", "Holy Grail" };
        private readonly string[] _buildDefinitionNames = { "My Build", "Test Build", "Integration", "Production-DE", "Production-EN" };
        private readonly Uri[] _buildSummaries = { new Uri("http://localhost/build_summary_1"), new Uri("http://localhost/build_summary_2"), new Uri("http://localhost/build_summary_3") };
        private readonly string[] _dropLocations = { "C:/builds/myBuild", "C:/builds/testBuild", "C:/builds/integration", "C:/builds/prod-de", "C:/builds/prod-EN" };
        private readonly string[] _users = { "me", "you", "him", "someone else", "someone completely unknown" };
        private readonly BuildStatus[] _status = { BuildStatus.InProgress, BuildStatus.NotStarted };
        private readonly BuildStatus[] _finishedStatus = { BuildStatus.Failed, BuildStatus.PartiallySucceeded, BuildStatus.Stopped, BuildStatus.Succeeded, BuildStatus.None };
        private readonly QueueStatus[] _queueStatus = { QueueStatus.Canceled, QueueStatus.Completed, QueueStatus.InProgress, QueueStatus.Postponed, QueueStatus.Queued, QueueStatus.Retry };

        public void Dispose()
        {
            _timer?.Stop();
        }

        private class TestQueuedBuild : IQueuedBuild
        {
            public IBuildDetail Build { get; set; }
            public QueueStatus Status { get; set; }

            public QueuePriority Priority { get; set; }
            public Guid BatchId { get; }
            public int Id { get; }
            public string TeamProject { get; }
            public IBuildController BuildController { get; }
            public Uri BuildControllerUri { get; }
            public IBuildDefinition BuildDefinition { get; }
            public Uri BuildDefinitionUri { get; }
            public ReadOnlyCollection<IBuildDetail> Builds { get; }
            public IBuildServer BuildServer { get; }
            public string CustomGetVersion { get; }
            public string DropLocation { get; }
            public GetOption GetOption { get; }
            public DateTime QueueTime { get; }
            public string ProcessParameters { get; }
            public int QueuePosition { get; }
            public BuildReason Reason { get; }
            public string RequestedBy { get; }
            public string RequestedByDisplayName { get; }
            public string RequestedFor { get; }
            public string RequestedForDisplayName { get; }
            public string ShelvesetName { get; }

            public event StatusChangedEventHandler StatusChanged;
            public event PollingCompletedEventHandler PollingCompleted;

            public void Cancel()
            {
            }

            public int CompareTo(IQueuedBuild other)
            {
                return 0;
            }

            public void Connect(int pollingInterval, int timeout, ISynchronizeInvoke synchronizingObject)
            {
            }

            public void Connect()
            {
            }

            public bool Copy(IQueuedBuild build, QueryOptions options)
            {
                return true;
            }

            public void Disconnect()
            {
            }

            public void Postpone()
            {
            }

            public void Refresh(QueryOptions queryOptions)
            {
            }

            public void Resume()
            {
            }

            public void Retry()
            {
            }

            public void Retry(Guid batchId)
            {
            }

            public void Retry(Guid batchId, QueuedBuildRetryOption retryOption)
            {
            }

            public void Save()
            {
            }

            public void StartNow()
            {
            }

            public void Wait()
            {
            }

            public bool WaitForBuildCompletion(TimeSpan pollingInterval, TimeSpan timeout)
            {
                return true;
            }

            public bool WaitForBuildCompletion(TimeSpan pollingInterval, TimeSpan timeout, ISynchronizeInvoke synchronizingObject)
            {
                return true;
            }

            public void WaitForBuildStart()
            {
            }

            public bool WaitForBuildStart(int pollingInterval, int timeout)
            {
                return true;
            }

            public virtual void OnStatusChanged(StatusChangedEventArgs e)
            {
                StatusChanged?.Invoke(this, e);
            }
        }

        private class TestBuildDetail : IBuildDetail
        {
            public BuildStatus Status { get; set; }
            public DateTime FinishTime { get; set; }
            //
            public string BuildNumber { get; set; }
            public BuildPhaseStatus CompilationStatus { get; set; }
            public string DropLocation { get; set; }
            public string DropLocationRoot { get; }
            public string LabelName { get; set; }
            public bool KeepForever { get; set; }
            public string LogLocation { get; set; }
            public string Quality { get; set; }

            public BuildPhaseStatus TestStatus { get; set; }
            public IBuildController BuildController { get; }
            public Uri BuildControllerUri { get; }
            public IBuildDefinition BuildDefinition { get; }
            public Uri BuildDefinitionUri { get; }
            public bool BuildFinished { get; }
            public IBuildServer BuildServer { get; }
            public IBuildInformation Information { get; }
            public string LastChangedBy { get; }
            public string LastChangedByDisplayName { get; }
            public DateTime LastChangedOn { get; }
            public string ProcessParameters { get; }
            public BuildReason Reason { get; }
            public ReadOnlyCollection<int> RequestIds { get; }
            public ReadOnlyCollection<IQueuedBuild> Requests { get; }
            public bool IsDeleted { get; }
            public string SourceGetVersion { get; set; }
            public DateTime StartTime { get; }
            public Uri Uri { get; }
            public string TeamProject { get; }
            public long? ContainerId { get; }
            public string RequestedBy { get; }
            public string RequestedFor { get; }
            public string ShelvesetName { get; }

            public event StatusChangedEventHandler StatusChanging;
            public event StatusChangedEventHandler StatusChanged;
            public event PollingCompletedEventHandler PollingCompleted;

            public void Connect(int pollingInterval, int timeout, ISynchronizeInvoke synchronizingObject)
            {
                throw new NotImplementedException();
            }

            public void Connect(int pollingInterval, ISynchronizeInvoke synchronizingObject)
            {
                throw new NotImplementedException();
            }

            public void Connect()
            {
                throw new NotImplementedException();
            }

            public IBuildDeletionResult Delete()
            {
                throw new NotImplementedException();
            }

            public IBuildDeletionResult Delete(DeleteOptions options)
            {
                throw new NotImplementedException();
            }

            public void Disconnect()
            {
                throw new NotImplementedException();
            }

            public void FinalizeStatus()
            {
                throw new NotImplementedException();
            }

            public void FinalizeStatus(BuildStatus status)
            {
                throw new NotImplementedException();
            }

            public void Refresh(string[] informationTypes, QueryOptions queryOptions)
            {
                throw new NotImplementedException();
            }

            public void RefreshAllDetails()
            {
                throw new NotImplementedException();
            }

            public void RefreshMinimalDetails()
            {
                throw new NotImplementedException();
            }

            public Guid RequestIntermediateLogs()
            {
                throw new NotImplementedException();
            }

            public void Save()
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }

            public void Wait()
            {
                throw new NotImplementedException();
            }

            public bool Wait(TimeSpan pollingInterval, TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public bool Wait(TimeSpan pollingInterval, TimeSpan timeout, ISynchronizeInvoke synchronizingObject)
            {
                throw new NotImplementedException();
            }
        }
    }
}
