using System.Threading.Tasks;

namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for a service that displays dialogs to the user
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a message dialog
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <returns>Task representing the dialog operation</returns>
        Task ShowMessageAsync(string message, string title = "Message");
        
        /// <summary>
        /// Shows an error dialog
        /// </summary>
        /// <param name="message">Error message to display</param>
        /// <param name="title">Dialog title</param>
        /// <returns>Task representing the dialog operation</returns>
        Task ShowErrorAsync(string message, string title = "Error");
        
        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Dialog title</param>
        /// <param name="confirmButtonText">Text for confirm button</param>
        /// <param name="cancelButtonText">Text for cancel button</param>
        /// <returns>True if confirmed, false if canceled</returns>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirm", string confirmButtonText = "OK", string cancelButtonText = "Cancel");
        
        /// <summary>
        /// Shows a file open dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">File filter (e.g., "Audio Files|*.mp3;*.wav")</param>
        /// <returns>Selected file path or null if canceled</returns>
        Task<string?> ShowOpenFileDialogAsync(string title = "Open File", string? filter = null);
        
        /// <summary>
        /// Shows a file save dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="defaultFileName">Default file name</param>
        /// <param name="filter">File filter (e.g., "Audio Files|*.mp3;*.wav")</param>
        /// <returns>Selected file path or null if canceled</returns>
        Task<string?> ShowSaveFileDialogAsync(string title = "Save File", string? defaultFileName = null, string? filter = null);
        
        /// <summary>
        /// Shows a folder browser dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <returns>Selected folder path or null if canceled</returns>
        Task<string> ShowFolderBrowserDialogAsync(string title = "Select Folder");

        /// <summary>
        /// Shows an input dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Message to display</param>
        /// <param name="initialValue">Optional initial value</param>
        /// <returns>Input text or null if canceled</returns>
        Task<string?> ShowInputDialogAsync(string title, string message, string? initialValue = null);
    }
}
