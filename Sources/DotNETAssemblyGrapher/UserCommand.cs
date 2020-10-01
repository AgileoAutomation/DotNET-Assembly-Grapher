using System;
using System.Windows.Input;

namespace DotNETAssemblyGrapher
{
    public class UICommand : ICommand
    {
        public UICommand(Func<bool> canExecute, Action execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private Func<bool> _canExecute;
        private Action _execute;

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _execute.Invoke();
        }
    }
}
