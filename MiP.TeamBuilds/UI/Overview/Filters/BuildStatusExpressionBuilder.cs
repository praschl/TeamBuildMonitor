using MiP.TeamBuilds.Providers;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.TeamFoundation.Build.Client;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public class BuildStatusExpressionBuilder : IExpressionBuilder
    {
        public bool CanParse(string prefix) => prefix == "s" || prefix == "status";

        public Expression<Func<BuildInfo, bool>> CreateExpression(string expression, ICollection<string> errors)
        {
            BuildStatus ConvertToStatus(string statusName)
            {
                if (Enum.TryParse(statusName, true, out BuildStatus result))
                    return result;

                errors.Add($"Status: Could not parse {statusName} to a build status.");
                return BuildStatus.All;
            }

            var parts = expression.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                errors.Add("Status: Missing comma separated list of status: " + string.Join(",", Enum.GetNames(typeof(BuildStatus))));
                return bi => true;
            }

            int filterValue = parts.Select(ConvertToStatus).Select(s => (int)s).Aggregate((a, b) => a | b);

            return buildInfo => Filter(buildInfo, filterValue);
        }

        private bool Filter(BuildInfo buildInfo, int filterValue)
        {
            return ((int)buildInfo.BuildStatus & filterValue) != 0;
        }
    }
}
