using System.Windows.Input;
using System;
using System.Windows.Markup;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    public class SetVisibilityCommand : MarkupExtension, ICommand
    {
        public Visibility VisibilityToSet { get; set; }
        public Visibility ToggleTo { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is UIElement element)
            {
                if (element.Visibility != VisibilityToSet)
                    element.Visibility = VisibilityToSet;
                else
                    element.Visibility = ToggleTo;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
