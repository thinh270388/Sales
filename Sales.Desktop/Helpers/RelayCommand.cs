using System.Windows.Input;

namespace Sales.Desktop.Helpers
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Action<object> _execute;
        private Func<object, bool>? _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute!(parameter!) || _canExecute == null;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter!);
        }
    }
}