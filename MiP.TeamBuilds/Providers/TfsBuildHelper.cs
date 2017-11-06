using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System.Threading.Tasks;

namespace MiP.TeamBuilds.Providers
{
    public class TfsBuildHelper : IDisposable
    {
        public TfsBuildHelper(Uri tfsUri)
        {
            _teamCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
        }

        private readonly TfsTeamProjectCollection _teamCollection;

        public Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync()
        {
            return Task.Run(() =>
            {
                var buildService = _teamCollection.GetService<IBuildServer>();

                var buildSpec = buildService.CreateBuildQueueSpec("*", "*");

                var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);

                return foundBuilds.QueuedBuilds.Select(Convert);
            });
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
                RequestedBy = build.RequestedBy,
                Status = build.Build.Status,
                BuildSummary = new Uri(buildSummary),
                DropLocation = !string.IsNullOrEmpty(dropLocation) ? dropLocation : null
            };
        }

        public void Dispose()
        {
            _teamCollection?.Dispose();
        }
    }
}
