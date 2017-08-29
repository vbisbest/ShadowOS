using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace core.ui.data
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action<object> action)
        {
            _Action = action;
        }

        private Action<object> _Action;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_Action != null)
            {
                _Action(parameter);
            }
        }

        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, null);
            }
        }
    }
}
