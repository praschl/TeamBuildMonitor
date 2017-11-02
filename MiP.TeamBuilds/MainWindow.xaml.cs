using System;
using System.Windows;

namespace MiP.TeamBuilds
{
    public partial class MainWindow : Window
    {     
        private MainViewModel MainModel { get; }

        public MainWindow(MainViewModel model)
        {
            MainModel = model;

            InitializeComponent();
        }
        
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow(MainModel);
            settings.ShowDialog();
            settings.Close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            MainModel.Initialize();
        }
    }
}
