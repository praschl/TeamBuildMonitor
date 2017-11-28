﻿using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using System.Linq;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO: Overview: Filter builds by text 
        // TODO: Overview: + Label "Showing 17 / 239 builds"
        // TODO: Overview: Filter builds by age
        // TODO: Overview: Second Listview for finished builds
        // TODO: Overview: ?Filter builds by state?
        // TODO: Overview: Menu for Droplocation, Buildsummary
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
            BuildsView.Refresh();
        }

        public ICommand SortCommand { get; set; }

        public OverviewViewModel(KnownBuildsViewModel knownBuildsViewModel)
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
            BuildsView.Filter = FilterBuilds;

            SortCommand = new SortCommandImpl(collectionViewSource);
        }

        private bool FilterBuilds(object obj)
        {
            if (!(obj is BuildInfo buildInfo))
                return false;

            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            return buildInfo.TeamProject.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.BuildDefinitionName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                || buildInfo.RequestedBy.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                ;
        }

        public class SortCommandImpl : ICommand
        {
            private readonly CollectionViewSource _collectionViewSource;

            public SortCommandImpl(CollectionViewSource collectionViewSource)
            {
                _collectionViewSource = collectionViewSource;
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

                _collectionViewSource.SortDescriptions.Clear();
                _collectionViewSource.SortDescriptions.Add(new SortDescription(newSort, direction));
                _collectionViewSource.LiveSortingProperties.Clear();
                _collectionViewSource.LiveSortingProperties.Add(newSort);
            }
        }
    }
}
