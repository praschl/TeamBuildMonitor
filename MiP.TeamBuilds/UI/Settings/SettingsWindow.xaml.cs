using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MiP.TeamBuilds.UI.Settings
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private string _tfsUrl;
        
        // TODO: Allow only one instance of Settings Window
        // TODO: Build Autostart setting
        public SettingsWindow()
        {
            _tfsUrl = Properties.Settings.Default.TfsUrl;
            InitializeComponent();
        }

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TfsUrl = _tfsUrl;
            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!DialogResult.HasValue)
                DialogResult = false;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
