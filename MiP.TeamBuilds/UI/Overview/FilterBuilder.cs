using LinqKit;
using MiP.TeamBuilds.Providers;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.TeamFoundation.Build.Client;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview
{
    public class FilterBuilder
    {
        public Func<BuildInfo, bool> ParseToFilter(string filterText)
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(filterText))
                return bi => true;

            // Remember: use LinqKit's PredicateBuilder to combine expressions

            // text "fulltext search"
            // x:<expression> like follows
            // a:FinishAge (a:xu) x=number u=unit (s=m=minutes,h=hours,d=days,y=years)
            // s:Status,Status

            var filterParts = filterText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            var expressions = filterParts.Select(GetExpression);

            var result = PredicateBuilder.New<BuildInfo>();
            foreach (var filterExpression in expressions)
            {
                result = result.And(filterExpression);
            }

            return result.Compile();
        }

        private readonly List<string> _errors = new List<string>();
        public IReadOnlyList<string> Errors => _errors;

        private Expression<Func<BuildInfo, bool>> GetExpression(string filter)
        {
            if (filter.StartsWith("a:"))
            {
                return CreateAgeExpression(filter.Substring(2));
            }
            else if (filter.StartsWith("s:"))
            {
                return CreateStatusExpression(filter.Substring(2));
            }
            else
            {
                return buildInfo =>
                buildInfo.TeamProject.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.BuildDefinitionName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedByDisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedBy.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private Expression<Func<BuildInfo, bool>> CreateStatusExpression(string filter)
        {
            BuildStatus ConvertToStatus(string statusName)
            {
                if (Enum.TryParse(statusName, true, out BuildStatus result))
                    return result;

                _errors.Add($"Status: Could not parse {statusName} to a build status.");
                return BuildStatus.None;
            }

            var parts = filter.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                _errors.Add("Status: Missing comma separated list of status: " + string.Join(",", Enum.GetNames(typeof(BuildStatus))));
                return bi => true;
            }

            int filterValue = parts.Select(ConvertToStatus).Select(s => (int)s).Aggregate((a, b) => a | b);

            return bi => ((int)bi.BuildStatus & filterValue) != 0;
        }

        private const string _validUnits = "smhdwMy";
        private Expression<Func<BuildInfo, bool>> CreateAgeExpression(string filter)
        {
            if (filter.Length < 2)
            {
                _errors.Add("Age: Missing value/unit in the form: 120s, 30m, 2h, 6d, 2w, 3M or 1y");
                return bi => true;
            }

            var unit = filter.Last();
            if (!_validUnits.Contains(unit))
                _errors.Add($"Age: <{unit}> is not a valid unit.");

            filter = filter.Substring(0, filter.Length - 1);

            if (!int.TryParse(filter, out int value))
            {
                _errors.Add($"Age: Could not parse {filter} to the value for unit {unit}");
                return bi => true;
            }

            TimeSpan maxAge = TimeSpan.Zero;
            switch (unit)
            {
                case 's': maxAge = TimeSpan.FromSeconds(value);
                    break;

                case 'm': maxAge = TimeSpan.FromMinutes(value);
                    break;

                case 'h': maxAge = TimeSpan.FromHours(value);
                    break;

                case 'd': maxAge = TimeSpan.FromDays(value);
                    break;

                case 'w':
                    maxAge = TimeSpan.FromDays(value * 7);
                    break;

                case 'M':
                    maxAge = TimeSpan.FromDays(value * 30);
                    break;

                case 'y':
                    maxAge = TimeSpan.FromDays(value * 365);
                    break;
            }

            return bi => bi.QueuedTime > DateTime.Now.Add(-maxAge);
        }
    }
}
