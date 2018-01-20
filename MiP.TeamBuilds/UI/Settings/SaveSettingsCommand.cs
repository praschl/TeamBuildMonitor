using MiP.TeamBuilds.Configuration;
using MiP.TeamBuilds.Providers;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Settings
{
    public class SaveSettingsCommand : ICommand
    {
        private readonly Func<SettingsViewModel> _viewModel;

        public SaveSettingsCommand(Func<SettingsViewModel> viewModel)
        {
            _viewModel = viewModel;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var viewModel = _viewModel();
            XmlConfig.Instance.TfsUrl = viewModel.TfsUrl;
            XmlConfig.Instance.MaxBuildAgeForDisplayDays = viewModel.MaxBuildAgeForDisplay;

            //
            XmlConfig.Save();

            AutoStartHelper.SetAutoStart(viewModel.AutoStart);

            if (parameter is SettingsWindow window)
                window.DialogResult = true;
        }
    }
}
