using MiP.TeamBuilds.Configuration;
using MiP.TeamBuilds.Providers;
using PropertyChanged;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Settings
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Generated OnPropertyChanged by Fody, Event has no subscribers at this time.")]
        public SettingsViewModel(SaveSettingsCommand saveSettingsCommand)
        {
            SaveSettingsCommand = saveSettingsCommand;

            TfsUrl = JsonConfig.Instance.TfsUrl;

            MaxBuildAgeForDisplay = JsonConfig.Instance.MaxBuildAgeForDisplayDays;

            AutoStart = AutoStartHelper.IsStartupItem();
        }

        // TODO: support multiple tfs urls

        public string TfsUrl { get; set; }
        public bool AutoStart { get; set; }
        public int MaxBuildAgeForDisplay { get; set; }

        public ICommand SaveSettingsCommand { get; }
    }
}
