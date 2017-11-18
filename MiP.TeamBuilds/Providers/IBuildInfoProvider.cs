using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiP.TeamBuilds.Providers
{
    public interface IBuildInfoProvider
    {
        void Initialize(Uri uri);

        Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync();
    }
}
