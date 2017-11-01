﻿using MiP.TeamBuilds.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MiP.TeamBuilds
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public SettingsWindow(IRestartTimer restartTimer)
        {
            _tfsUrl = Settings.Default.TfsUrl;
            InitializeComponent();
            _restartTimer = restartTimer;
        }

        private string _tfsUrl;
        private readonly IRestartTimer _restartTimer;

        public string TfsUrl
        {
            get => _tfsUrl;
            set
            {
                if (_tfsUrl == value)
                    return;

                _tfsUrl = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.TfsUrl = _tfsUrl;
            Settings.Default.Save();
            Close();
            _restartTimer.RestartTimer();
        }
    }
}
