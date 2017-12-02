using MiP.TeamBuilds.Providers;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public interface IExpressionBuilder
    {
        bool CanParse(string prefix);

        Expression<Func<BuildInfo, bool>> CreateExpression(string expression, ICollection<string> errors);
    }
}
