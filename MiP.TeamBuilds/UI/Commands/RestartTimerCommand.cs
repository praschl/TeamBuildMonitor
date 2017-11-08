using MiP.TeamBuilds.UI.Main;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class RestartTimerCommand : ICommand
    {
        private readonly Func<IRefreshBuildsTimer> _refreshBuildsTimer;

        public RestartTimerCommand(Func<IRefreshBuildsTimer> refreshBuildsTimer)
        {
            _refreshBuildsTimer = refreshBuildsTimer;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _refreshBuildsTimer().RestartTimer();
        }
    }
}
