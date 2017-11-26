using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    /// <summary>
    /// Interaction logic for OverviewWindow.xaml
    /// </summary>
    public partial class OverviewWindow : Window
    {
        public OverviewViewModel OverviewViewModel { get; }

        public OverviewWindow(OverviewViewModel overviewViewModel)
        {
            OverviewViewModel = overviewViewModel;

            InitializeComponent();
        }
    }
}
