using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiP.TeamBuilds.Providers
{
    // TODO: allow a special url like "about:demo", 
    // and for that url resolve a fake instance of IBuildInfoProvider which randomly returns build states.

    public interface IBuildInfoProvider
    {
        void Initialize(Uri uri);

        Task<IEnumerable<BuildInfo>> GetCurrentBuildsAsync();
    }
}
