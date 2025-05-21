using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TrialWorld.Presentation.Commands
{
    /// <summary>
    /// Interface for asynchronous commands that support cancellation and progress reporting
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Executes the command asynchronously
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ExecuteAsync();
        
        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Generic interface for asynchronous commands that support cancellation and progress reporting
    /// </summary>
    /// <typeparam name="T">The type of parameter passed to the command</typeparam>
    public interface IAsyncCommand<T> : ICommand
    {
        /// <summary>
        /// Executes the command asynchronously with the specified parameter
        /// </summary>
        /// <param name="parameter">The parameter to pass to the command</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ExecuteAsync(T parameter);
        
        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}
