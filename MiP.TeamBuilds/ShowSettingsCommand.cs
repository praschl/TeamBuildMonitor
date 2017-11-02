using System;
using System.Windows.Input;

namespace MiP.TeamBuilds
{
    public class ShowSettingsCommand : ICommand
    {
        private readonly Func<SettingsWindow> _settingsWindowFactory;

        public ShowSettingsCommand(Func<SettingsWindow> settingsWindowFunc)
        {
            _settingsWindowFactory = settingsWindowFunc;
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
            settings.Close();
        }
    }
}
