using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using MiP.TeamBuilds.UI.Commands;
using System.Windows.Markup;
using System.Globalization;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO: Overview: Filter: Expressions Help -> Display under
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

        private readonly FilterBuilder _filterBuilder = new FilterBuilder();
        private Func<BuildInfo, bool> _currentFilter = bi => true;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView BuildsView { get; set; }
        public bool IsBusy { get; set; }

        public string FilterText { get; set; }
        private void OnFilterTextChanged() // called by Fody when FilterText changes
        {
            CreateFilterFuncFromText();
        }
        public string FilterErrorText { get; set; }

        public ICommand SortCommand { get; }
        public ICommand OpenBuildSummaryCommand { get; }
        public ICommand RefreshOldBuildsCommand { get; }

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

            // somehow, the _collectionViewSource and _collectionView lose the current Filter 
            // when _collectionViewSource.SortDescriptions.Clear() is called, so we need to set it again,
            // thus: () => BuildsView.Filter = FilterBuilds, TODO: get rid of that, its ugly.
            SortCommand = new SortCommand(collectionViewSource, () => BuildsView.Filter = FilterBuilds);

            OpenBuildSummaryCommand = openBuildSummaryCommand;
            RefreshOldBuildsCommand = refreshOldBuildsCommand;
        }

        private void CreateFilterFuncFromText()
        {
            string filterText = (FilterText ?? "").Trim();

            if (string.IsNullOrWhiteSpace(filterText))
                BuildsView.Filter = null;
            else
                BuildsView.Filter = FilterBuilds;

            _currentFilter = _filterBuilder.ParseToFilter(filterText);
            FilterErrorText = string.Join(Environment.NewLine, _filterBuilder.Errors);

            // TODO: When AgeFilter is set, refresh the filter (not the data) every minute - BuildsView.Refresh() should be sufficient

            BuildsView.Refresh();
        }

        private bool FilterBuilds(object obj)
        {
            if (!(obj is BuildInfo buildInfo))
                return false;

            return _currentFilter(buildInfo);
        }
    }

    public class StringEmptyToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility EmptyResult { get; set; } = Visibility.Collapsed;
        public Visibility ElseResult { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return EmptyResult;
            if (string.Empty.Equals(value))
                return EmptyResult;

            return ElseResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
