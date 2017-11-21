using System.Windows;

namespace MiP.TeamBuilds.UI.Ambient
{
    /// <summary>
    /// The ambient window will never be shown and just acts as a container for the TrayIcon
    /// </summary>
    public partial class AmbientWindow : Window
    {
        public AmbientViewModel AmbientViewModel { get; }

        public AmbientWindow(AmbientViewModel ambientViewModel)
        {
            AmbientViewModel = ambientViewModel;

            InitializeComponent();
        }
    }
}
