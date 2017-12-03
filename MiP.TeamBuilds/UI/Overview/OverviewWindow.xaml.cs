using MiP.TeamBuilds.Providers;
using System.Windows;
using System.Windows.Controls;

namespace MiP.TeamBuilds.UI.Overview
{
    public partial class OverviewWindow : Window
    {
        public OverviewViewModel OverviewViewModel { get; }

        public OverviewWindow(OverviewViewModel overviewViewModel)
        {
            OverviewViewModel = overviewViewModel;

            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.Content is BuildInfo info)
                OverviewViewModel.OpenBuildSummaryCommand.Execute(info.BuildSummary);
        }

        internal void Unhide()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            BringIntoView();
            Activate();
        }
    }
}
