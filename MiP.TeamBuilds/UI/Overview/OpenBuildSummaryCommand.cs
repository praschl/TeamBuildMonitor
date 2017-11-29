using System.Windows.Input;
using System;
using System.Diagnostics;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OpenBuildSummaryCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is Uri url)
                Process.Start(url.ToString());
        }
    }
}
