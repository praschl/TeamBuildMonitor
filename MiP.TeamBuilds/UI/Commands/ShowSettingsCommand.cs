using MiP.TeamBuilds.UI.Settings;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        private bool _canExecute = true;

        private readonly Func<SettingsWindow> _settingsWindowFactory;
        private readonly Func<RebuildTfsProviderCommand> _restartTimerCommand;

        public ShowSettingsCommand(Func<SettingsWindow> settingsWindowFunc, Func<RebuildTfsProviderCommand> restartTimerCommand)
        {
            _settingsWindowFactory = settingsWindowFunc;
            _restartTimerCommand = restartTimerCommand;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter)
        {
            _canExecute = false;
            OnCanExecuteChanged(EventArgs.Empty);

            try
            {
                SettingsWindow currentInstance = _settingsWindowFactory();
                if (currentInstance.ShowDialog() == true)
                {
                    _restartTimerCommand().Execute(null);
                }
            }
            finally
            {
                _canExecute = true;
                OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }
    }
}
