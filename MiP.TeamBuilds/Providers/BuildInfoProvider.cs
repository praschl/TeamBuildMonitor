using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Framework.Client;
using System.Collections.Concurrent;
using Microsoft.TeamFoundation.Framework.Common;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace MiP.TeamBuilds.Providers
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Only managed resources used")]
    public class BuildInfoProvider : IBuildInfoProvider, IDisposable
    {
        public delegate BuildInfoProvider Factory(Uri tfsUri);

        private readonly ConcurrentDictionary<string, string> _userIdToUserName = new ConcurrentDictionary<string, string>();
        private Uri _tfsUri;

        private readonly ConcurrentBag<TfsTeamProjectCollection> _teamProjectCollections = new ConcurrentBag<TfsTeamProjectCollection>();

        public async Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync()
        {
            await InitializeTeamCollectionsAsync().ConfigureAwait(false);

            var tasks = _teamProjectCollections.Select(c => GetCurrentBuildsAsync(c));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks.Select(t => t.Result).SelectMany(bi => bi);
        }

        private Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync(TfsTeamProjectCollection collection)
        {
            return Task.Run(() =>
            {
                var buildService = collection.GetService<IBuildServer>();

                var buildSpec = buildService.CreateBuildQueueSpec("*", "*");

                var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);
                PreloadUserNames(foundBuilds);

                return foundBuilds.QueuedBuilds.Select(qb => Convert(qb, collection));
            });
        }

        private Task InitializeTeamCollectionsAsync()
        {
            return Task.Run(() =>
            {
                if (_teamProjectCollections.Count > 0)
                    return;

                foreach (var item in GetCollections())
                {
                    _teamProjectCollections.Add(item);
                }
            });
        }

        private IEnumerable<TfsTeamProjectCollection> GetCollections()
        {
            var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(_tfsUri);

            var tpcService = configurationServer.GetService<ITeamProjectCollectionService>();

            var configurationServerNode = configurationServer.CatalogNode;

            var tpcNodes = configurationServerNode.QueryChildren(
                    new Guid[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);

            var collectionNames = tpcNodes.Select(n => n.Resource.DisplayName);

            var tfsTeamProjectCollections = collectionNames
                .Select(cn => TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(_tfsUri, cn)))
                .ToArray();

            return tfsTeamProjectCollections;
        }

        private void PreloadUserNames(IQueuedBuildQueryResult foundBuilds)
        {
            foreach (var collection in _teamProjectCollections)
            {
                var ims = collection.GetService<IIdentityManagementService>();
                var unknownUserIds = foundBuilds.QueuedBuilds.Select(b => b.RequestedBy).Except(_userIdToUserName.Keys).ToArray();

                var users = ims.ReadIdentities(IdentitySearchFactor.AccountName, unknownUserIds, MembershipQuery.None, ReadIdentityOptions.ExtendedProperties);

                foreach (var user in users.SelectMany(T => T))
                {
                    _userIdToUserName.TryAdd(user.UniqueName, user.DisplayName);
                }
            }
        }

        private BuildInfo Convert(IQueuedBuild build, TfsTeamProjectCollection collection)
        {
            string collectionUri = build.BuildServer.TeamProjectCollection.Uri.ToString();
            string project = build.BuildDefinition.TeamProject;
            string id = build.Build.Uri.ToString().Split('/').Last();

            string buildSummary = $"{collectionUri}/{project}/_build?_a=summary&buildId={id}";
            string dropLocation = build.Build.DropLocation;
            if (string.IsNullOrEmpty(dropLocation))
                dropLocation = build.DropLocation;

            return new BuildInfo(build)
            {
                Id = Invariant($"[{collection}]/{build.Id}"),
                TeamProject = build.TeamProject,
                BuildDefinitionName = build.BuildDefinition.Name,
                ServerItems = build.BuildDefinition.Workspace.Mappings.Select(m => m.ServerItem).ToArray(),
                RequestedBy = GetRequestedBy(build.RequestedBy),
                Status = build.Build.Status,
                BuildSummary = new Uri(buildSummary),
                DropLocation = !string.IsNullOrEmpty(dropLocation) ? dropLocation : null,
                QueuedTime = build.QueueTime,
                FinishTime = build.Build.FinishTime
            };
        }

        private string GetRequestedBy(string requestedBy)
        {
            if (_userIdToUserName.TryGetValue(requestedBy, out string displayName))
                return displayName;
            else
                return requestedBy;

        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Only managed resources used.")]
        public void Dispose()
        {
            foreach (var item in _teamProjectCollections)
            {
                item.Dispose();
            }
        }

        public void Initialize(Uri uri)
        {
            _tfsUri = uri;
        }
    }
}
