using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for platform-agnostic file dialog operations
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Show a file open dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        Task<string?> ShowOpenDialogAsync(string title = "Open File", string? initialDirectory = null, string? filter = null);

        /// <summary>
        /// Show a file open dialog (synchronous version)
        /// </summary>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <returns>Selected file path or empty string if cancelled</returns>
        string OpenFile(string filter, string title = "Open File", string? initialDirectory = null);

        /// <summary>
        /// Show a file save dialog
        /// </summary>
        /// <param name="defaultFileName">Default file name</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="defaultExtension">Default file extension</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        Task<string?> ShowSaveDialogAsync(string defaultFileName, string? initialDirectory = null, string? defaultExtension = null, string? filter = null);

        /// <summary>
        /// Show a folder browser dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <returns>Selected directory path or null if cancelled</returns>
        Task<string?> ShowFolderBrowserDialogAsync(string title = "Select Folder", string? initialDirectory = null);
    }
}
