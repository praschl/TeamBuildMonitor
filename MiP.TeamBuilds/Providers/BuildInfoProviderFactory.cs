using Autofac.Features.AttributeFilters;
using Autofac.Features.OwnedInstances;
using System;

namespace MiP.TeamBuilds.Providers
{
    public class BuildInfoProviderFactory
    {
        private readonly Func<Owned<IBuildInfoProvider>> _buildInfoProvider;
        private readonly Func<Owned<IBuildInfoProvider>> _testProvider;

        public BuildInfoProviderFactory(
            [KeyFilter("http")] Func<Owned<IBuildInfoProvider>> buildInfoProvider,
            [KeyFilter("demo")] Func<Owned<IBuildInfoProvider>> testProvider)
        {
            _buildInfoProvider = buildInfoProvider;
            _testProvider = testProvider;
        }

        public Owned<IBuildInfoProvider> GetProvider(Uri uri)
        {
            Owned<IBuildInfoProvider> provider = null;

            if (uri.Scheme == "about" && string.Equals(uri.LocalPath, "demo", StringComparison.InvariantCultureIgnoreCase))
                provider = _testProvider();
            else
                provider = _buildInfoProvider();

            provider.Value.Initialize(uri);
            return provider;
        }
    }
}
