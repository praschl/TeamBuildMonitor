using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.TeamFoundation.Framework.Common;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace MiP.TeamBuilds.Providers
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Only managed resources used")]
    public class BuildInfoProvider : IBuildInfoProvider, IDisposable
    {
        private TimeSpan MaxBuildAgeForDisplay => Properties.Settings.Default.MaxBuildAgeForDisplay;

        private Uri _tfsUri;

        private readonly ConcurrentDictionary<Guid, TfsTeamProjectCollection> _teamProjectCollections = new ConcurrentDictionary<Guid, TfsTeamProjectCollection>();

        private static readonly IEnumerable<BuildInfo> _emptyResult = Enumerable.Empty<BuildInfo>();

        public void Initialize(Uri uri)
        {
            _tfsUri = uri;
        }

        public async Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync()
        {
            await InitializeTeamCollectionsAsync().ConfigureAwait(false);

            if (_disposed) return _emptyResult; // dispose may be run from the UI thread.
            var tasks = _teamProjectCollections.Select(c => GetCurrentBuildsAsync(c.Value));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.Select(t => t.Result).SelectMany(bi => bi);
        }

        private Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync(TfsTeamProjectCollection collection)
        {
            return Task.Run(() =>
            {
                var buildService = collection.GetService<IBuildServer>();

                var buildSpec = buildService.CreateBuildQueueSpec("*", "*");
                buildSpec.QueryOptions = QueryOptions.Definitions;

                var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);

                return foundBuilds.QueuedBuilds.Select(qb => Convert(qb, collection));
            });
        }

        public async Task<IEnumerable<BuildInfo>> GetRecentlyFinishedBuildsAsync()
        {
            await InitializeTeamCollectionsAsync().ConfigureAwait(false);

            if (_disposed) return _emptyResult; // dispose may be run from the UI thread.
            var tasks = _teamProjectCollections.Select(c => GetRecentlyFinishedBuildsAsync(c.Value));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.Select(t => t.Result).SelectMany(bi => bi);
        }

        private Task<IEnumerable<BuildInfo>> GetRecentlyFinishedBuildsAsync(TfsTeamProjectCollection collection)
        {
            return Task.Run(() =>
            {
                var buildService = collection.GetService<IBuildServer>();

                var buildSpec = buildService.CreateBuildQueueSpec("*", "*");
                buildSpec.QueryOptions = QueryOptions.Definitions;
                buildSpec.CompletedWindow = MaxBuildAgeForDisplay;
                buildSpec.Status = QueueStatus.Completed;

                var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);

                return foundBuilds.QueuedBuilds.Select(qb => Convert(qb, collection));
            });
        }

        private Task InitializeTeamCollectionsAsync()
        {
            return Task.Run(() =>
            {
                if (_teamProjectCollections.Count > 0)
                    return;

                foreach (var collection in GetCollections())
                {
                    // since this is called from GetCurrentBuildsAsync and GetRecentlyFinishedBuildsAsync
                    // and those will run asynchronously, one of them might have added the collection already.
                    _teamProjectCollections.TryAdd(collection.InstanceId, collection);
                }
            });
        }

        private IEnumerable<TfsTeamProjectCollection> GetCollections()
        {
            var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(_tfsUri);

            var configurationServerNode = configurationServer.CatalogNode;

            var tpcNodes = configurationServerNode.QueryChildren(
                    new Guid[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);

            var collectionNames = tpcNodes.Select(n => n.Resource.DisplayName);

            var pathSeparator = _tfsUri.AbsoluteUri.EndsWith("/") ? string.Empty : "/";

            var tfsTeamProjectCollections = collectionNames
                .Select(cn => TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(_tfsUri + pathSeparator + cn)))
                .ToArray();

            return tfsTeamProjectCollections;
        }

        private BuildInfo Convert(IQueuedBuild build, TfsTeamProjectCollection collection)
        {
            string collectionUri = build.BuildServer.TeamProjectCollection.Uri.ToString();
            string project = build.BuildDefinition.TeamProject;
            string id = build.Build?.Uri.ToString().Split('/').Last();

            string buildSummary = $"{collectionUri}/{project}/_build?_a=summary&buildId={id}";
            string dropLocation = build.Build?.DropLocation;
            if (string.IsNullOrEmpty(dropLocation))
                dropLocation = build.DropLocation;

            // TODO: display icon for QueueStatus when build.BuildStatus == None

            return new BuildInfo(build)
            {
                Id = Invariant($"[{collection}]/{build.Id}"),
                TeamProject = build.TeamProject,
                BuildDefinitionName = build.BuildDefinition.Name,
                ServerItems = build.BuildDefinition.Workspace.Mappings.Select(m => m.ServerItem).ToArray(),
                RequestedByDisplayName = build.RequestedByDisplayName,
                RequestedBy = build.RequestedBy,
                QueueStatus = build.Status,
                BuildStatus = build.Build?.Status ?? BuildStatus.None,
                BuildSummary = new Uri(buildSummary),
                DropLocation = !string.IsNullOrEmpty(dropLocation) ? dropLocation : null,
                QueuedTime = build.QueueTime,
                FinishTime = build.Build?.FinishTime ?? DateTime.MinValue
                // sometimes, QueuedTime is within 1 hour after FinishTime... is there a problem with UTC and non UTC Time ?
            };
        }

        private bool _disposed;

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Only managed resources used.")]
        public void Dispose()
        {
            _disposed = true;
            foreach (var collection in _teamProjectCollections)
            {
                collection.Value.Dispose();
            }
        }
    }
}
