using MiP.TeamBuilds.Configuration;
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

                var settings = JsonConfig.Instance.OverviewWindowSettings;

                if (settings.Width > 0 && settings.Height > 0)
                {
                    _window.Left = settings.Left;
                    _window.Top = settings.Top;
                    _window.Width = settings.Width;
                    _window.Height = settings.Height;
                }
                if (settings.Maximized)
                    _window.WindowState = WindowState.Maximized;

                _window.Closing += Window_Closing;
                _window.Closed += Window_Closed;
                _window.Show();
            }
            else
            {
                _window.Unhide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = JsonConfig.Instance.OverviewWindowSettings;

            settings.Maximized = _window?.WindowState == WindowState.Maximized;
            if (!settings.Maximized)
            {
                settings.Left = _window.Left;
                settings.Top = _window.Top;
                settings.Width = _window.Width;
                settings.Height = _window.Height;
            }

            JsonConfig.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _window.Closed -= Window_Closed;
            _window = null;
        }
    }
}
