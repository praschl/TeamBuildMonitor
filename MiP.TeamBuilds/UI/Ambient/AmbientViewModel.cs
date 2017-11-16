using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Notifications;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using static MiP.TeamBuilds.UI.Notifications.KnownBuildsViewModel;
using ToastNotifications;
using ToastNotifications.Core;
using MiP.TeamBuilds.IoC;
using Autofac;
using MiP.TeamBuilds.UI.CompositeNotifications;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class AmbientViewModel : INotifyPropertyChanged
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Generated OnPropertyChanged by Fody, Event has no subscribers at this time.")]
        public AmbientViewModel(ShowSettingsCommand showSettingsCommand, QuitCommand quitCommand, KnownBuildsViewModel knownBuildsViewModel)
        {
            ShowSettingsCommand = showSettingsCommand;
            QuitCommand = quitCommand;
            KnownBuildsViewModel = knownBuildsViewModel;

            CurrentBuildsView = CollectionViewSource.GetDefaultView(knownBuildsViewModel.Builds);
            CurrentBuildsView.SortDescriptions.Add(new SortDescription(nameof(BuildInfo.BuildDefinitionName), ListSortDirection.Ascending));

            /*
             Note to self: CollectionView only subscribes to the PropertyChanged-event, not to CollectionChanged.
             ObservableCollection does not have a PropertyChanged-event, so we subscribe ourself to CollectionChanged,
             and raise a PropertyChanged, to update the view.
             */
            knownBuildsViewModel.Builds.CollectionChanged +=
                (o, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBuildsView)));

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
        public ICollectionView CurrentBuildsView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void Initialize()
        {
            KnownBuildsViewModel.Initialize();
        }

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
