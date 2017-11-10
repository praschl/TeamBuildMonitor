using MiP.TeamBuilds.Providers;
using PropertyChanged;
using System;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Settings
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        public SettingsViewModel(SaveSettingsCommand saveSettingsCommand)
        {
            SaveSettingsCommand = saveSettingsCommand;

            TfsUrl = Properties.Settings.Default.TfsUrl;

            AutoStart = AutoStartHelper.IsStartupItem();
        }

        public string TfsUrl
        {
            get;
            set;
        }

        public bool AutoStart
        {
            get;
            set;
        }

        public ICommand SaveSettingsCommand { get; }
    }
}
