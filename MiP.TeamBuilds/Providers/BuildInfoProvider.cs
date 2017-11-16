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

namespace MiP.TeamBuilds.Providers
{
    // TODO: for DEBUG builds:
    // make interface + fake implementation which returns fake builds.
    // create a new build definition (copy from DEBUG): "DEMO" and
    // register fake implementation in the DEMO build

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Only managed resources used")]
    public class BuildInfoProvider : IDisposable
    {
        public delegate BuildInfoProvider Factory(Uri tfsUri);

        public BuildInfoProvider(Uri tfsUri)
        {
            _teamCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
        }

        private readonly TfsTeamProjectCollection _teamCollection;

        private readonly ConcurrentDictionary<string, string> _userIdToUserName = new ConcurrentDictionary<string, string>();

        public Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync()
        {
            return Task.Run(() =>
            {
                var buildService = _teamCollection.GetService<IBuildServer>();

                var buildSpec = buildService.CreateBuildQueueSpec("*", "*");

                var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);
                PreloadUserNames(foundBuilds);

                return foundBuilds.QueuedBuilds.Select(Convert);
            });
        }

        private void PreloadUserNames(IQueuedBuildQueryResult foundBuilds)
        {
            var ims = _teamCollection.GetService<IIdentityManagementService>();
            var unknownUserIds = foundBuilds.QueuedBuilds.Select(b => b.RequestedBy).Except(_userIdToUserName.Keys).ToArray();

            var users = ims.ReadIdentities(IdentitySearchFactor.AccountName, unknownUserIds, MembershipQuery.None, ReadIdentityOptions.ExtendedProperties);

            foreach (var user in users.SelectMany(T => T))
            {
                _userIdToUserName.TryAdd(user.UniqueName, user.DisplayName);
            }
        }

        private BuildInfo Convert(IQueuedBuild build)
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
                Id = build.Id,
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
            _teamCollection?.Dispose();
        }
    }
}
