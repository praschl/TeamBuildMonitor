using LinqKit;
using MiP.TeamBuilds.Providers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public class FilterBuilder : IFilterBuilder
    {
        private readonly List<string> _errors = new List<string>();
        private readonly IExpressionBuilder[] _expressionBuilders;

        public FilterBuilder(IEnumerable<IExpressionBuilder> expressionBuilders)
        {
            _expressionBuilders = expressionBuilders.ToArray();
        }

        public IReadOnlyCollection<string> GetErrors() => _errors;

        public Func<BuildInfo, bool> ParseToFilter(string filterText)
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(filterText))
                return bi => true;

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

        private Expression<Func<BuildInfo, bool>> GetExpression(string filter)
        {
            var parts = filter.Split(new[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                string prefix = parts[0].Trim();
                string value = parts[1].Trim();

                var builder = Array.Find(_expressionBuilders, b => b.CanParse(prefix));
                if (builder != null)
                    return builder.CreateExpression(value, _errors);
            }

            return buildInfo =>
                buildInfo.TeamProject.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.BuildDefinitionName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedByDisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedBy.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
