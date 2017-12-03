using System;
using System.Windows.Input;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OpenOverviewCommand : ICommand
    {
        private readonly Func<OverviewWindow> _overviewWindow;
        private OverviewWindow _window = null;

        public OpenOverviewCommand(Func<OverviewWindow> overviewWindow)
        {
            _overviewWindow = overviewWindow;
        }

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (_window == null)
            {
                _window = _overviewWindow();
                _window.Closed += Window_Closed;
                _window.Show();
            }
            else
            {
                _window.Unhide();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _window.Closed -= Window_Closed;
            _window = null;
        }
    }
}
