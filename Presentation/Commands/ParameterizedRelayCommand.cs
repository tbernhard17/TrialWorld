using System;
using System.Windows.Input;

namespace TrialWorld.Presentation.Commands
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates with an object parameter. The default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class ParameterizedRelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedRelayCommand"/> class
        /// </summary>
        /// <param name="execute">The execution logic with parameter</param>
        /// <param name="canExecute">The execution status logic with parameter</param>
        public ParameterizedRelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this command can execute in its current state
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data, 
        /// this parameter can be set to null.
        /// </param>
        /// <returns>True if this command can be executed; otherwise, false</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data, 
        /// this parameter can be set to null.
        /// </param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler? CanExecuteChanged;
    }
}
