using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiP.TeamBuilds
{
    public class TfsBuildHelper : IDisposable
    {
        public TfsBuildHelper(Uri tfsUri)
        {
            _teamCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
        }

        private readonly TfsTeamProjectCollection _teamCollection;

        public IEnumerable<BuildInfo> GetCurrentBuilds()
        {            
            var buildService = _teamCollection.GetService<IBuildServer>();
            
            var buildSpec = buildService.CreateBuildQueueSpec("*", "*");
            buildSpec.CompletedWindow = TimeSpan.FromSeconds(30); // TODO: remove completed builds and finished flag once the model connects to each build

            var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);
            
            var result = foundBuilds.QueuedBuilds.Select(b =>
                          new BuildInfo
                          {
                              Id = b.Id,
                              TeamProject = b.TeamProject,
                              BuildDefinitionName = b.BuildDefinition.Name,
                              ServerItems = b.BuildDefinition.Workspace.Mappings.Select(m => m.ServerItem).ToArray(),
                              RequestedBy = b.RequestedBy,
                              Finished = b.Build.BuildFinished,
                              Status = b.Build.Status
                          });
            
            return result;
        }

        public void Dispose()
        {
            _teamCollection?.Dispose();
        }
    }
}
