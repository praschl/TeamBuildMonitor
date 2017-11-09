using MiP.TeamBuilds.UI.Notifications;
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

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

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
