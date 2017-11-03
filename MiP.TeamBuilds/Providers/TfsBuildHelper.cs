﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace MiP.TeamBuilds.Providers
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
            // TODO: make this async and do not block ui

            var buildService = _teamCollection.GetService<IBuildServer>();
            
            var buildSpec = buildService.CreateBuildQueueSpec("*", "*");
            
            var foundBuilds = buildService.QueryQueuedBuilds(buildSpec);

            var result = foundBuilds.QueuedBuilds.Select(b =>
                          new BuildInfo(b)
                          {
                              Id = b.Id,
                              TeamProject = b.TeamProject,
                              BuildDefinitionName = b.BuildDefinition.Name,
                              ServerItems = b.BuildDefinition.Workspace.Mappings.Select(m => m.ServerItem).ToArray(),
                              RequestedBy = b.RequestedBy,
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