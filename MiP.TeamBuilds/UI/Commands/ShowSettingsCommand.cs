using MiP.TeamBuilds.UI.Main;
using MiP.TeamBuilds.UI.Settings;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class ShowSettingsCommand : ICommand
    {
        private readonly Func<SettingsWindow> _settingsWindowFactory;
        private readonly MainViewModel _mainViewModel;

        public ShowSettingsCommand(Func<SettingsWindow> settingsWindowFunc, MainViewModel mainViewModel)
        {
            _settingsWindowFactory = settingsWindowFunc;
            _mainViewModel = mainViewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SettingsWindow settings = _settingsWindowFactory();
            if (settings.ShowDialog() == true)
            {
                _mainViewModel.Initialize();
            }
        }
    }
}
