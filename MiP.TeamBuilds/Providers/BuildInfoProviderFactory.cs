using Autofac.Features.OwnedInstances;
using System;

namespace MiP.TeamBuilds.Providers
{
    public class BuildInfoProviderFactory
    {
        private readonly Func<Owned<IBuildInfoProvider>> _buildInfoProvider;

        public BuildInfoProviderFactory(Func<Owned<IBuildInfoProvider>> buildInfoProvider)
        {
            _buildInfoProvider = buildInfoProvider;
        }

        public Owned<IBuildInfoProvider> GetProvider()
        {
            return _buildInfoProvider();
        }
    }
}
