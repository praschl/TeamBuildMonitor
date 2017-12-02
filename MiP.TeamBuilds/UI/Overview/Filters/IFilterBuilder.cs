using MiP.TeamBuilds.Providers;
using System;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public interface IFilterBuilder
    {
        Func<BuildInfo, bool> ParseToFilter(string filterText);

        IReadOnlyCollection<string> GetErrors();
    }
}
