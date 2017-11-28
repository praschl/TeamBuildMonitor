using System.Windows.Input;
using System;
using System.Windows.Markup;
using MiP.TeamBuilds.UI.Overview;

namespace MiP.TeamBuilds.UI.Commands
{
    public class CloseCommand : MarkupExtension, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is OverviewWindow w) w.Close();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
