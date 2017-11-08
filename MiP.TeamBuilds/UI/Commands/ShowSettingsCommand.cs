using MiP.TeamBuilds.UI.Main;
using MiP.TeamBuilds.UI.Settings;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        private readonly Func<SettingsWindow> _settingsWindowFactory;
        private readonly KnownBuildsModel _knownBuildsModel;

        public ShowSettingsCommand(Func<SettingsWindow> settingsWindowFunc, KnownBuildsModel knownBuildsModel)
        {
            _settingsWindowFactory = settingsWindowFunc;
            _knownBuildsModel = knownBuildsModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SettingsWindow settings = _settingsWindowFactory();
            settings.ShowDialog();
            if (settings.DialogResult == true)
            {
                _knownBuildsModel.RestartTimer();
            }
        }
    }
}
