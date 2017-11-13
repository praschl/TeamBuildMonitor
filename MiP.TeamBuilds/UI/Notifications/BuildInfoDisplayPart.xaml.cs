using System.Windows;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    public partial class BuildInfoDisplayPart : NotificationDisplayPart
    {
        private readonly BuildInfoMessage _viewModel;

        public BuildInfoDisplayPart(BuildInfoMessage message)
        {
            InitializeComponent();

            _viewModel = message;
            DataContext = message;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            _viewModel.Close();
        }

        public override MessageOptions GetOptions()
        {
            return _viewModel.Options;
        }
    }
}
