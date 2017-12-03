﻿using System;
using System.Windows;
using System.Windows.Input;

namespace MiP.TeamBuilds.UI.Commands
{
    // TODO: check all commands to see which can be turned into a MarkupExtension, less XAML

    public class QuitCommand : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

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
