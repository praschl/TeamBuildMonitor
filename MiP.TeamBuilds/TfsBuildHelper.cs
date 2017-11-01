using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using MiP.TeamBuilds.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiP.TeamBuilds
{
    public class TfsBuildHelper : IDisposable
    {
        public TfsBuildHelper()
        {
            string tfsUrl = Settings.Default.TfsUrl;

            // "http://sv-int-tfs-02:8080/tfs/DefaultCollection"

            _teamCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsUrl));
        }

        private TfsTeamProjectCollection _teamCollection;

        public IEnumerable<BuildInfo> GetCurrentBuilds()
        {            
            var buildService = _teamCollection.GetService<IBuildServer>();
            
            var buildSpec = buildService.CreateBuildQueueSpec("*", "*");
            buildSpec.CompletedWindow = TimeSpan.FromMinutes(5);

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
