using MiP.TeamBuilds.Providers;
using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.UI.Overview
{
    public class BuildInfoComparer : IComparer
    {
        private Func<BuildInfo, IComparable> _property = b => b.BuildDefinitionName;
        private string _propertyName;

        private static Dictionary<string, Func<BuildInfo, IComparable>> _getters = new Dictionary<string, Func<BuildInfo, IComparable>>
        {
            { nameof(BuildInfo.TeamProject), b => b.TeamProject },
            { nameof(BuildInfo.BuildDefinitionName), b => b.BuildDefinitionName },
            { nameof(BuildInfo.QueuedTime), b => b.QueuedTime },
            { nameof(BuildInfo.RequestedByDisplayName), b => b.RequestedByDisplayName },
            { nameof(BuildInfo.BuildStatus), b => GetBuildStatusSort(b.BuildStatus) },
            { nameof(BuildInfo.FinishTime), b => b.FinishTime == DateTime.MinValue ? DateTime.MaxValue : b.FinishTime },
            { nameof(BuildInfo.Duration), b => b.Duration }
        };

        private static Dictionary<BuildStatus, int> _buildStatusSort = new Dictionary<BuildStatus, int>
        {
            { BuildStatus.None, 0 },
            { BuildStatus.NotStarted, 1 },
            { BuildStatus.InProgress, 2 },
            { BuildStatus.Stopped, 3 },
            { BuildStatus.Failed, 4 },
            { BuildStatus.PartiallySucceeded, 5 },
            { BuildStatus.Succeeded, 6 },
        };

        private static int GetBuildStatusSort(BuildStatus status)
        {
            if (_buildStatusSort.TryGetValue(status, out int sort))
                return sort;

            return 100;
        }

        public string PropertyName
        {
            get => _propertyName;
            set
            {
                _propertyName = value;

                if (_getters.TryGetValue(_propertyName, out Func<BuildInfo, IComparable> func))
                    _property = func;
            }
        }

        public ListSortDirection Direction { get; set; }

        public int Compare(object x, object y)
        {
            var left = x as BuildInfo;
            var right = y as BuildInfo;

            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            return Compare(left, right);
        }

        private int Compare(BuildInfo left, BuildInfo right)
        {
            var compareResult = _property(left).CompareTo(_property(right));

            if (Direction == ListSortDirection.Descending)
                return compareResult * -1;

            return compareResult;
        }
    }
}
