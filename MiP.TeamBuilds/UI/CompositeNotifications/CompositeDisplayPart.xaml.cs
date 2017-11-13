using System.Windows;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.CompositeNotifications
{
    /// <summary>
    /// Interaction logic for ErrorDisplayPart.xaml
    /// </summary>
    public partial class CompositeDisplayPart : NotificationDisplayPart
    {
        private readonly CompositeMessage _viewModel;

        public CompositeDisplayPart(CompositeMessage message)
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
