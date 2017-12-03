using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Overview.Filters;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO: Overview: Filter: "Advanced filter" window
        // TODO: Overview: Refresh button to reload (and refilter) old builds (F5 already works).
        // TODO: Overview: When sorting by FinishTime, filter running builds to top instead of bottom
        // TODO: Overview: + Label "Showing 17 / 239 builds"
        // TODO: Overview: Second Listview for finished builds
        // TODO: Overview: Menu for Droplocation
        // TODO: Overview: Display progress based on older known builds
        // TODO: Overview: Menu for Stop build, Retry build
        // TODO: Overview: Display direction of sort in list headers

        private readonly KnownBuildsViewModel _knownBuildsViewModel;

        private readonly IFilterBuilder _filterBuilder;
        private Func<BuildInfo, bool> _currentFilter = bi => true;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView BuildsCollectionView { get; set; }
        public bool IsBusy { get; set; }
        public string FilterText { get; set; }
        private void OnFilterTextChanged() // called by Fody when FilterText changes
        {
            CreateFilterFuncFromText();
        }

        public string FilterErrorText { get; set; }
        public string FilterHelpText => OverviewResources.FilterHelp;

        public ICommand OpenBuildSummaryCommand { get; }
        public ICommand RefreshOldBuildsCommand { get; }

        public OverviewViewModel(KnownBuildsViewModel knownBuildsViewModel, OpenBuildSummaryCommand openBuildSummaryCommand, RefreshOldBuildsCommand refreshOldBuildsCommand, IFilterBuilder filterBuilder)
        {
            _knownBuildsViewModel = knownBuildsViewModel;

            OpenBuildSummaryCommand = openBuildSummaryCommand;
            RefreshOldBuildsCommand = refreshOldBuildsCommand;
            _filterBuilder = filterBuilder;

            _knownBuildsViewModel.Builds.CollectionChanged +=
                          (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildsCollectionView)));

            _knownBuildsViewModel.IsBusyChanged += (o, e) => IsBusy = _knownBuildsViewModel.IsBusy;
            IsBusy = _knownBuildsViewModel.IsBusy;

            /* NOTE TO SELF: CollectionViewSource.GetDefaultView returns the same instance of collection view 
             * for the same collection. So, to use different collection views (for different controls) on the same collection instance,
             * the collection view has to be instantiated by at least using manually created CollectionViewSource or by creating the ICollectionView by hand
             */

            BuildsCollectionView = new ListCollectionView(knownBuildsViewModel.Builds)
            {
                IsLiveFiltering = true,
                Filter = FilterBuilds,
                IsLiveSorting = true,
                SortDescriptions = { new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending) },
            };

            //var timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(5);
            //timer.Tick += delegate { }
            // TODO: update date/time properties periodically
            //timer.Start();
        }

        private void CreateFilterFuncFromText()
        {
            string filterText = (FilterText ?? "").Trim();

            _currentFilter = _filterBuilder.ParseToFilter(filterText);
            FilterErrorText = string.Join(Environment.NewLine, _filterBuilder.GetErrors());

            // TODO: When AgeFilter is set, refresh the filter (not the data) every minute - BuildsView.Refresh() should be sufficient

            BuildsCollectionView.Refresh();
        }

        private bool FilterBuilds(object obj)
        {
            if (!(obj is BuildInfo buildInfo))
                return false;

            return _currentFilter(buildInfo);
        }
    }
}
