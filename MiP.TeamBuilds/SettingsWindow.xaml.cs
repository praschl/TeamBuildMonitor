using MiP.TeamBuilds.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MiP.TeamBuilds
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public SettingsWindow()
        {
            _tfsUrl = Settings.Default.TfsUrl;
            InitializeComponent();
        }

        private string _tfsUrl;
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

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Settings.Default.TfsUrl = _tfsUrl;
            Settings.Default.Save();
        }
    }
}
