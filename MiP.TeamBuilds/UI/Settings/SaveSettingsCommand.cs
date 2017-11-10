using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Settings
{
    public class SaveSettingsCommand : ICommand
    {
        private readonly SettingsViewModel _viewModel;

        public SaveSettingsCommand(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            Properties.Settings.Default.TfsUrl = _viewModel.TfsUrl;
            //Properties.Settings.Default.Save();

            if (parameter is SettingsWindow window)
                window.DialogResult = true;
        }
    }
}
