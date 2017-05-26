using System;
using System.Windows.Input;

namespace Rewriter.Commands
{
    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action<object> execute)
            : base(execute)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute) 
            : base(execute, canExecute)
        {
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute)
            :this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) != false;

        public void Execute(object parameter) => _execute((T)parameter);
    }
}