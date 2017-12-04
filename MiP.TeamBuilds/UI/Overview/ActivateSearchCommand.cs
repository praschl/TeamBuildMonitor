using MiP.TeamBuilds.Providers;
using MiP.TeamBuilds.UI.Notifications;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System;
using MiP.TeamBuilds.UI.Commands;
using MiP.TeamBuilds.UI.Overview.Filters;
using System.Windows.Markup;
using System.Windows;

namespace MiP.TeamBuilds.UI.Overview
{
    public class ActivateSearchCommand : MarkupExtension, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is UIElement uiElement && uiElement.Focusable)
                uiElement.Focus();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
