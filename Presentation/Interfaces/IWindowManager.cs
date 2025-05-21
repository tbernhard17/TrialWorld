using System;
using System.Threading.Tasks;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for managing application windows
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows a window with the specified view model
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model</typeparam>
        /// <returns>True if the dialog was confirmed, false otherwise</returns>
        bool ShowDialog<TViewModel>() where TViewModel : class;
        
        /// <summary>
        /// Shows a window with the specified view model and parameters
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model</typeparam>
        /// <param name="parameters">Parameters to pass to the view model</param>
        /// <returns>True if the dialog was confirmed, false otherwise</returns>
        bool ShowDialog<TViewModel>(object parameters) where TViewModel : class;
        
        /// <summary>
        /// Shows a non-modal window
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model</typeparam>
        void ShowWindow<TViewModel>() where TViewModel : class;
    }
}
