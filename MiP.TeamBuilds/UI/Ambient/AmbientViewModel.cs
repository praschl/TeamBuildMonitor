using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Notifications;
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class AmbientViewModel
    {
        public AmbientViewModel(ShowSettingsCommand showSettingsCommand, QuitCommand quitCommand, KnownBuildsViewModel knownBuildsViewModel)
        {
            ShowSettingsCommand = showSettingsCommand;
            QuitCommand = quitCommand;
            KnownBuildsViewModel = knownBuildsViewModel;

            CurrentBuildsView = CollectionViewSource.GetDefaultView(knownBuildsViewModel.Builds);
            CurrentBuildsView.SortDescriptions.Add(new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending));

            // TODO: the next will be relevant when the collection may contain finished builds.
            //CurrentBuildsView.Filter = CurrentBuildsFilter;
            //if (CurrentBuildsView is ICollectionViewLiveShaping live)
            //{
            //    live.LiveFilteringProperties.Add(nameof(BuildInfo.FinishTime));
            //}
        }

        public ShowSettingsCommand ShowSettingsCommand { get; }
        public QuitCommand QuitCommand { get; }
        public KnownBuildsViewModel KnownBuildsViewModel { get; }

        public ICollectionView CurrentBuildsView { get; }

        internal void Initialize()
        {
            KnownBuildsViewModel.Initialize();
        }

        private bool CurrentBuildsFilter(object buildInfo)
        {
            return buildInfo is BuildInfo bi && bi.FinishTime == DateTime.MinValue;
        }
    }
}
