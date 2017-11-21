using MiP.TeamBuilds.UI.Notifications;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Ambient
{
    public class SleepCommand : ICommand
    {
        private readonly Func<ITimerRefreshViewModel> _buildsTimer;

        public SleepCommand(Func<ITimerRefreshViewModel> buildsTimer)
        {
            _buildsTimer = buildsTimer;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (int.TryParse((string)parameter, out int minutes))
                _buildsTimer().StopRefreshingFor(minutes);
        }
    }
}
