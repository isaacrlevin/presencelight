using System;
using System.Windows.Input;

namespace PresenceLight.ViewModels
{
    public class RelayCommand : ICommand
    {
        private Action<object?> _executeAction { get; set; }
        private Predicate<object?>? _canExecuteAction { get; set; }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = default)
        {
            _executeAction = execute;
            _canExecuteAction = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecuteAction?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _executeAction(parameter);
        }
    }
}
