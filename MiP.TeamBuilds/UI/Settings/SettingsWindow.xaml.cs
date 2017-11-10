using System.ComponentModel;
using System.Windows;

namespace MiP.TeamBuilds.UI.Settings
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; set; }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!DialogResult.HasValue)
                DialogResult = false;
        }
    }
}
