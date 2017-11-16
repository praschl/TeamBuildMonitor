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

    public interface IBuildInfoProvider
    {
        Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync();
    }
}
