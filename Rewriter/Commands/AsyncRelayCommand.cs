using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Rewriter.Commands
{
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public AsyncRelayCommand(Func<T, Task> execute)
            :this(execute, null)
        {
        }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) != false;

        public async void Execute(object parameter) => await _execute((T)parameter);
    }
}