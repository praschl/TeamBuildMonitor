using MiP.TeamBuilds.UI.Main;
using MiP.TeamBuilds.UI.Settings;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        private bool _canExecute = true;

        private readonly Func<SettingsWindow> _settingsWindowFactory;
        private readonly KnownBuildsModel _knownBuildsModel;

        public ShowSettingsCommand(Func<SettingsWindow> settingsWindowFunc, KnownBuildsModel knownBuildsModel)
        {
            _settingsWindowFactory = settingsWindowFunc;
            _knownBuildsModel = knownBuildsModel;
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
                    _knownBuildsModel.RestartTimer();
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
