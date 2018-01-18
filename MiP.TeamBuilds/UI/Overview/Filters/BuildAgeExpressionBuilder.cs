using MiP.TeamBuilds.Providers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MiP.TeamBuilds.UI.Overview.Filters
{
    public class BuildAgeExpressionBuilder : IExpressionBuilder
    {
        private const string _validUnits = "smhdwMy";

        public bool CanParse(string prefix) => prefix == "a" || prefix == "age";

        public Expression<Func<BuildInfo, bool>> CreateExpression(string expression, ICollection<string> errors)
        {
            if (expression.Length < 2)
            {
                errors.Add("Age: Missing value/unit in the form: 120s, 30m, 2h, 6d, 2w, 3M or 1y");
                return bi => true;
            }

            var unit = expression.Last();
            if (!_validUnits.Contains(unit))
                errors.Add($"Age: <{unit}> is not a valid unit.");

            expression = expression.Substring(0, expression.Length - 1);

            if (!int.TryParse(expression, out int value))
            {
                errors.Add($"Age: Could not parse {expression} to the value for unit {unit}");
                return bi => true;
            }

            TimeSpan maxAge = TimeSpan.Zero;
            switch (unit)
            {
                case 's':
                    maxAge = TimeSpan.FromSeconds(value);
                    break;

                case 'm':
                    maxAge = TimeSpan.FromMinutes(value);
                    break;

                case 'h':
                    maxAge = TimeSpan.FromHours(value);
                    break;

                case 'd':
                    maxAge = TimeSpan.FromDays(value);
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

            return buildInfo => Filter(buildInfo, -maxAge);
        }

        private bool Filter(BuildInfo buildInfo, TimeSpan maxAge)
        {
            return buildInfo.FinishTime > DateTime.Now.Add(maxAge) || buildInfo.FinishTime == DateTime.MinValue;
        }
    }
}
