using PropertyChanged;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Settings
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        private readonly Func<SaveSettingsCommand> _saveSettingsCommand;

        public SettingsViewModel(Func<SaveSettingsCommand> saveSettingsCommand)
        {
            TfsUrl = Properties.Settings.Default.TfsUrl;
            _saveSettingsCommand = saveSettingsCommand;
        }

        public string TfsUrl
        {
            get;
            set;
        }

        public ICommand SaveSettingsCommand => _saveSettingsCommand();
    }
}
