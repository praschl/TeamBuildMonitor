using System.Windows;
using ToastNotifications.Core;

namespace MiP.TeamBuilds.UI.Notifications
{
    /// <summary>
    /// Interaction logic for ErrorDisplayPart.xaml
    /// </summary>
    public partial class CompositeDisplayPart : NotificationDisplayPart
    {
        private readonly CompositeMessage _viewModel;

        public CompositeDisplayPart(CompositeMessage error)
        {
            InitializeComponent();

            _viewModel = error;
            DataContext = error;
        }
        
        private void OnClose(object sender, RoutedEventArgs e)
        {

            _viewModel.Close();
        }

        public override MessageOptions GetOptions()
        {
            return this._viewModel.Options;
        }
    }
}
