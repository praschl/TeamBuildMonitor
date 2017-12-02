using System.Windows.Input;
using System;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;

namespace MiP.TeamBuilds.UI.Overview
{
    public class OpenPopupCommand : MarkupExtension, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is Popup popup)
                popup.IsOpen = true;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
