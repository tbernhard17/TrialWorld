using System.Threading.Tasks;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for dialog services that show UI prompts and windows
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a confirmation dialog with Yes/No or OK/Cancel buttons
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The dialog title</param>
        /// <returns>True if confirmed, false otherwise</returns>
        bool ShowConfirmation(string message, string title = "Confirmation");
        
        /// <summary>
        /// Shows an error dialog
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="title">The dialog title</param>
        void ShowError(string message, string title = "Error");
        
        /// <summary>
        /// Shows an informational message dialog
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The dialog title</param>
        void ShowMessage(string message, string title = "Information");
    }
}
