using System;
using System.Windows;

namespace MiP.TeamBuilds.UI.Main
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
            MainModel.ShowSettingsCommand.Execute(null);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            MainModel.Initialize();
        }
    }
}
