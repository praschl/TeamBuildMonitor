using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using MiP.TeamBuilds.UI.Commands;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO: Overview: Refresh button to reload (and refilter) old builds (F5 already works).
        // TODO: Overview: ?Filter builds by state?
        // TODO: Overview: When sorting by FinishTime, filter running builds to top instead of bottom
        // TODO: Overview: + Label "Showing 17 / 239 builds"
        // TODO: Overview: Second Listview for finished builds
        // TODO: Overview: Menu for Droplocation
        // TODO: Overview: Display progress based on older known builds
        // TODO: Overview: Menu for Stop build, Retry build
        // TODO: Overview: Display direction of sort in list headers

        private KnownBuildsViewModel _knownBuildsViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView BuildsView { get; set; }
        public bool IsBusy { get; set; }

        public string FilterText { get; set; }
        private void OnFilterTextChanged() // called by Fody when FilterText changes
        {
            SetFilter();
        }

        public ICommand SortCommand { get; }
        public ICommand OpenBuildSummaryCommand { get; }
        public ICommand RefreshOldBuildsCommand { get; }

        public List<string> FilterBuildByAgeItems { get; } = new List<string>
        {
            "age",
            "5 minutes",
            "15 minutes",
            "1 hour",
            "2 hours",
            "8 hours",
            "1 day",
            "2 days",
            "1 week",
            "1 month",
            "1 year"
        };
        private static TimeSpan[] _filterBuildAgeTimeSpans =
        {
            TimeSpan.FromDays(100*365),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(8),
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(2),
            TimeSpan.FromDays(7),
            TimeSpan.FromDays(30),
            TimeSpan.FromDays(365)
        };

        public int SelectedFilterBuildByAgeIndex { get; set; }
        public void OnSelectedFilterBuildByAgeIndexChanged()
        {
            SetFilter();
        }

        public OverviewViewModel(KnownBuildsViewModel knownBuildsViewModel, OpenBuildSummaryCommand openBuildSummaryCommand, RefreshOldBuildsCommand refreshOldBuildsCommand)
        {
            _knownBuildsViewModel = knownBuildsViewModel;
            _knownBuildsViewModel.Builds.CollectionChanged +=
                          (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildsView)));

            _knownBuildsViewModel.IsBusyChanged += (o, e) => IsBusy = _knownBuildsViewModel.IsBusy;
            IsBusy = _knownBuildsViewModel.IsBusy;

            /* NOTE TO SELF: CollectionViewSource.GetDefaultView returns the same instance of collection view 
             * for the same collection. So, to use different collection views (for different controls) on the same collection instance,
             * the collection view has to be instantiated like this: 
             */
            var collectionViewSource = new CollectionViewSource
            {
                Source = knownBuildsViewModel.Builds,
                SortDescriptions = { new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending) },
                IsLiveSortingRequested = true,
            };
            BuildsView = collectionViewSource.View;
            SortCommand = new SortCommandImpl(collectionViewSource, SetFilter);
            OpenBuildSummaryCommand = openBuildSummaryCommand;
            RefreshOldBuildsCommand = refreshOldBuildsCommand;
        }

        private void SetFilter()
        {
            if (string.IsNullOrWhiteSpace(FilterText) && SelectedFilterBuildByAgeIndex == 0)
                BuildsView.Filter = null;
            else
                BuildsView.Filter = FilterBuilds;

            // TODO: When AgeFilter is set, refresh the filter (not the data) every minute - BuildsView.Refresh() should be sufficient

            BuildsView.Refresh();
        }

        private bool FilterBuilds(object obj)
        {
            if (!(obj is BuildInfo buildInfo))
                return false;

            string filterText = (FilterText ?? "").Trim();
            DateTime earliestQueueTime = DateTime.Now - _filterBuildAgeTimeSpans[SelectedFilterBuildByAgeIndex];

            return (string.IsNullOrWhiteSpace(filterText)
                || buildInfo.TeamProject.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.BuildDefinitionName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedByDisplayName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedBy.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                &&
                buildInfo.QueuedTime > earliestQueueTime
                ;
        }

        // TODO: refactor SortCommandImpl: make own file, class name (Impl), make injectable.
        public class SortCommandImpl : ICommand
        {
            private readonly CollectionViewSource _collectionViewSource;
            private readonly Action _setFilter;

            public SortCommandImpl(CollectionViewSource collectionViewSource, Action setFilter)
            {
                _collectionViewSource = collectionViewSource;
                _setFilter = setFilter;
            }

            public CollectionView CollectionView { get; set; }

            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                if (!(parameter is string newSort))
                    return;

                var currentSort = _collectionViewSource.SortDescriptions.First();
                var direction = ListSortDirection.Ascending;

                if (newSort == currentSort.PropertyName)
                    direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                using (_collectionViewSource.DeferRefresh())
                {
                    _collectionViewSource.SortDescriptions.Clear();
                    _collectionViewSource.SortDescriptions.Add(new SortDescription(newSort, direction));
                    _collectionViewSource.LiveSortingProperties.Clear();
                    _collectionViewSource.LiveSortingProperties.Add(newSort);
                }
                _setFilter();
            }
        }
    }
}
