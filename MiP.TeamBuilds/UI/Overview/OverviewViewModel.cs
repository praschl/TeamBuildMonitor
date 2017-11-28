﻿using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using PropertyChanged;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO: Overview: Implement sorting (SortCommand)
        // TODO: Overview: Filter builds by text + Label "Showing 17 / 239 builds"
        // TODO: Overview: Second Listview for finished builds
        // TODO: Overview: Menu for Droplocation, Buildsummary
        // TODO: Overview: Display progress based on older known builds
        // TODO: Overview: Menu for Stop build, Retry build

        public KnownBuildsViewModel KnownBuildsViewModel { get; set; }

        public ICommand SortCommand => null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView BuildsView { get; set; }

        public OverviewViewModel(KnownBuildsViewModel knownBuildsViewModel)
        {
            KnownBuildsViewModel = knownBuildsViewModel;
            KnownBuildsViewModel.Builds.CollectionChanged +=
                          (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildsView)));

            /* NOTE TO SELF: CollectionViewSource.GetDefaultView returns the same instance of collection view 
             * for the same collection. So, to use different collection views (for different controls) on the same collection instance,
             * the collection view has to be instantiated like this: 
             */
            BuildsView = new CollectionViewSource
            {
                Source = knownBuildsViewModel.Builds,
                SortDescriptions = { new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending) }
            }.View;
        }
    }
}
