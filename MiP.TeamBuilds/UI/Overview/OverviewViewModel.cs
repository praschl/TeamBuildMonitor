using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OverviewViewModel : INotifyPropertyChanged
    {
        // TODO Overview: Keep finished builds (filter them in tooltip)
        // TODO Overview: When initializing, get older builds
        // TODO Overview: Filter builds by text + Label "Showing 17 / 239 builds"
        // TODO Overview: Second Listview for finished builds
        // TODO Overview: Menu for Droplocation, Buildsummary
        // TODO Overview: Display progress based on older known builds
        // TODO Overview: Menu for Stop build, Retry build

        private readonly KnownBuildsViewModel _knownBuildsViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView BuildsView { get; set; }

        public OverviewViewModel(KnownBuildsViewModel knownBuildsViewModel)
        {
            _knownBuildsViewModel = knownBuildsViewModel;

            BuildsView = CollectionViewSource.GetDefaultView(knownBuildsViewModel.Builds);
            BuildsView.SortDescriptions.Add(new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending));

            knownBuildsViewModel.Builds.CollectionChanged +=
                          (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildsView)));
        }
    }
}
