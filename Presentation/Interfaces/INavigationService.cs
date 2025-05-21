using System.Threading.Tasks;
using TrialWorld.Presentation.ViewModels;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for navigation service that manages view/page navigation
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a view by its name
        /// </summary>
        /// <param name="viewName">The name of the view to navigate to</param>
        void Navigate(string viewName);
        
        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel to navigate to</typeparam>
        void NavigateToViewModel<TViewModel>() where TViewModel : class;
        
        /// <summary>
        /// Gets a value indicating whether navigation back is possible
        /// </summary>
        bool CanGoBack { get; }
        
        /// <summary>
        /// Navigates to the previous view in the navigation stack
        /// </summary>
        void GoBack();
    }
}
