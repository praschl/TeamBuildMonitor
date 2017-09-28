using System.Windows;

namespace MiP.TeamBuilds
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BuildDefinitions_Click(object sender, RoutedEventArgs e)
        {
            mainView.Visibility = Visibility.Visible;
        }

        private void Builds_Click(object sender, RoutedEventArgs e)
        {
            mainView.Visibility = Visibility.Visible;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            mainView.Visibility = Visibility.Collapsed;
            settingsView.Visibility = Visibility.Visible;
        }

        private void SettingsApply_Click(object sender, RoutedEventArgs e)
        {
            mainView.Visibility = Visibility.Visible;
            settingsView.Visibility = Visibility.Collapsed;
        }
    }
}
