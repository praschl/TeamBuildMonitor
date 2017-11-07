using System;
using System.Windows;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    public class QuitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Application.Current.Shutdown();
        }
    }
}
