using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Notifications;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using MiP.TeamBuilds.UI.Overview;
using System.Globalization;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class AmbientViewModel : INotifyPropertyChanged
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Generated OnPropertyChanged by Fody, Event has no subscribers at this time.")]
        public AmbientViewModel(ShowSettingsCommand showSettingsCommand, QuitCommand quitCommand, SleepCommand sleepCommand,
            ITimerRefreshViewModel timerRefreshViewModel, KnownBuildsViewModel knownBuildsViewModel, OpenOverviewCommand openOverviewCommand)
        {
            ShowSettingsCommand = showSettingsCommand;
            QuitCommand = quitCommand;
            SleepCommand = sleepCommand;
            OpenOverviewCommand = openOverviewCommand;
            TimerRefreshViewModel = timerRefreshViewModel;
            KnownBuildsViewModel = knownBuildsViewModel;

            /* NOTE TO SELF: CollectionView only subscribes to the PropertyChanged-event, not to CollectionChanged.
             * ObservableCollection does not have a PropertyChanged-event, so we subscribe ourself to CollectionChanged,
             * and raise a PropertyChanged, to update the view.
             */
            knownBuildsViewModel.Builds.CollectionChanged +=
                (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBuildsView)));

            var collectionViewSource = new CollectionViewSource
            {
                Source = knownBuildsViewModel.Builds,
                IsLiveFilteringRequested = true,
                LiveFilteringProperties = { nameof(BuildInfo.FinishTime) },
                SortDescriptions = { new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending) },
            };
            CurrentBuildsView = collectionViewSource.View;
            CurrentBuildsView.Filter = CurrentBuildsFilter;
        }

        public ShowSettingsCommand ShowSettingsCommand { get; }
        public QuitCommand QuitCommand { get; }
        public SleepCommand SleepCommand { get; }
        public OpenOverviewCommand OpenOverviewCommand { get; }
        public KnownBuildsViewModel KnownBuildsViewModel { get; }
        public ITimerRefreshViewModel TimerRefreshViewModel { get; }

        public ICollectionView CurrentBuildsView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CurrentBuildsFilter(object buildInfo)
        {
            return buildInfo is BuildInfo bi && bi.FinishTime == DateTime.MinValue;
        }

#if DEBUG
        public Visibility DebugVisibility => Visibility.Visible;
        public ICommand TestMessages => new TestMessagesCommand();
#else
        public Visibility DebugVisibility =>   Visibility.Collapsed;
#endif

    }
}
