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
        // TODO: Save Filter with xctk:DropDownButton + DropDownContent
        // -- also save sort direction with filter
        // TODO: Save Column Order Settings
        // TODO: Save Column & Widths & Window Position
        // TODO: Overview: Menu for Droplocation
        // TODO: Overview: Menu for Stop build, Retry build
        // TODO: Overview: Display progress based on older known builds (Requires setting for how old builds to get for the statistics)
        // -- make statistics a separate collection of builds in KnownBuildsViewModel
        // TODO: Overview: Filter: "Advanced filter" window
        // TODO: Overview: + Label "Showing 17 / 239 builds"
        // TODO: Overview: Second Listview for finished builds (required factoring out the control + filter + some properties)
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

            _knownBuildsViewModel.IsBusyChanged += (o, e) => IsBusy = _knownBuildsViewModel.IsBusy;
            IsBusy = _knownBuildsViewModel.IsBusy;

            /* NOTE TO SELF: CollectionViewSource.GetDefaultView returns the same instance of collection view 
             * for the same collection. So, to use different collection views (for different controls) on the same collection instance,
             * the collection view has to be instantiated by at least using manually created CollectionViewSource or by creating the ICollectionView by hand
             */

            BuildsCollectionView = new ListCollectionView(knownBuildsViewModel.Builds)
            {
                IsLiveFiltering = true,
                LiveFilteringProperties = { nameof(BuildInfo.FinishTime), nameof(BuildInfo.BuildStatus) },
                Filter = FilterBuilds,
                IsLiveSorting = true,
                //SortDescriptions = { new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending) },
                CustomSort = _buildInfoComparer,
            };
        }

        private BuildInfoComparer _buildInfoComparer = new BuildInfoComparer();

        private void CreateFilterFuncFromText()
        {
            string filterText = (FilterText ?? "").Trim();

            _currentFilter = _filterBuilder.ParseToFilter(filterText);
            FilterErrorText = string.Join(Environment.NewLine, _filterBuilder.GetErrors());
            
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
