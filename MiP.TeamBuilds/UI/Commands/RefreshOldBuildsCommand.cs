using System.Windows.Input;
using System;
using MiP.TeamBuilds.UI.Notifications;

namespace MiP.TeamBuilds.UI.Commands
{
    public class RefreshOldBuildsCommand : ICommand
    {
        private readonly KnownBuildsViewModel _knownBuildsViewModel;

        public RefreshOldBuildsCommand(KnownBuildsViewModel knownBuildsViewModel)
        {
            _knownBuildsViewModel = knownBuildsViewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _knownBuildsViewModel.RebuildTfsProvider();
        }
    }
}
