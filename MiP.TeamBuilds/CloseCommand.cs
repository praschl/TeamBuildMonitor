using System;
using System.Windows.Input;

namespace MiP.TeamBuilds
{
    public class CloseCommand : ICommand
    {
        public CloseCommand(MainWindow mainForm)
        {
            _mainForm = mainForm;
        }

        private MainWindow _mainForm;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _mainForm.Close();
        }
    }
}
