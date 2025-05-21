using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TrialWorld.Presentation.Commands
{
    /// <summary>
    /// A command that asynchronously relays its functionality to other objects by invoking delegates.
    /// Implements .NET async patterns best practices including proper exception handling.
    /// </summary>
    public class AsyncRelayCommand : IAsyncCommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Occurs when changes occur that affect whether the command can execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous function to execute when the command is invoked.</param>
        /// <param name="canExecute">The function that determines whether the command can execute in its current state.</param>
        /// <exception cref="ArgumentNullException">Thrown when the execute function is null.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. Not used in this implementation.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute());
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">Data used by the command. Not used in this implementation.</param>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            await ExecuteAsync().ConfigureAwait(true);
        }
        
        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                // Execute the task
                await _execute().ConfigureAwait(true);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// A generic asynchronous command that relays its functionality to other objects by invoking delegates.
    /// Supports parameter passing and implements .NET async patterns best practices.
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to the command.</typeparam>
    public class AsyncRelayCommand<T> : IAsyncCommand<T>
    {
        private readonly Func<T, Task> _execute;
        private readonly Predicate<T>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Occurs when changes occur that affect whether the command can execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous function to execute when the command is invoked.</param>
        /// <param name="canExecute">The function that determines whether the command can execute in its current state.</param>
        /// <exception cref="ArgumentNullException">Thrown when the execute function is null.</exception>
        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. Must be of type T or null.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            if (_isExecuting)
            {
                return false;
            }

            if (_canExecute == null)
            {
                return true;
            }

            if (parameter == null && typeof(T).IsValueType)
            {
                return false;
            }

            return _canExecute((T?)parameter!);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">Data used by the command. Must be of type T or null.</param>
        public async void Execute(object? parameter)
        {
            if (parameter is T typedParameter && CanExecute(parameter))
            {
                await ExecuteAsync(typedParameter).ConfigureAwait(true);
            }
        }
        
        /// <summary>
        /// Executes the command asynchronously with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(T parameter)
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                // Execute the task
                await _execute(parameter).ConfigureAwait(true);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
