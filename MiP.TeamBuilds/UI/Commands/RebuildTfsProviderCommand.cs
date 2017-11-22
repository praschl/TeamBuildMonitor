using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class RebuildTfsProviderCommand : ICommand
    {
        private readonly Func<KnownBuildsViewModel> _knownBuildsViewModel;

        public RebuildTfsProviderCommand(Func<KnownBuildsViewModel> knownBuildsViewModel)
        {
            _knownBuildsViewModel = knownBuildsViewModel;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _knownBuildsViewModel().RebuildTfsProvider();
        }
    }
}
