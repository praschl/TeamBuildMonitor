using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using MiP.TeamBuilds.UI.Commands;

namespace MiP.TeamBuilds.UI.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Func<ShowSettingsCommand> _showSettingsCommandFactory;

        public MainViewModel(Func<ShowSettingsCommand> showSettingsCommandFactory, KnownBuildsModel knownBuildsModel)
        {
            _showSettingsCommandFactory = showSettingsCommandFactory;
            KnownBuildsModel = knownBuildsModel;
        }

        public KnownBuildsModel KnownBuildsModel { get; }

        public ICommand ShowSettingsCommand => _showSettingsCommandFactory();
        public ICommand QuitCommand => new QuitCommand();

        public void Initialize()
        {
            RestartTimer();
        }

        private void RestartTimer()
        {
            KnownBuildsModel.RestartTimer();
        }

        //INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
