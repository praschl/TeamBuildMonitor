using MiP.TeamBuilds.Providers;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public class RequestedByExpressionBuilder : IExpressionBuilder
    {
        public bool CanParse(string prefix) => prefix == "r" || prefix == "requestedBy";

        public Expression<Func<BuildInfo, bool>> CreateExpression(string expression, ICollection<string> errors)
        {
            return buildInfo => Filter(buildInfo, expression);
        }

        private bool Filter(BuildInfo buildInfo, string requestedBy)
        {
            return buildInfo.RequestedByDisplayName.IndexOf(requestedBy, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedBy.IndexOf(requestedBy, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

}
