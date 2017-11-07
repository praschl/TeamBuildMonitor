using System;
using System.Windows;

namespace MiP.TeamBuilds.UI.Main
{
    public partial class MainWindow : Window
    {
        public MainViewModel MainModel { get; }

        public MainWindow(MainViewModel model)
        {
            MainModel = model;

            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            MainModel.Initialize();
        }
    }
}
