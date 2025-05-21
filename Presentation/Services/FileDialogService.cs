using TrialWorld.Presentation.Interfaces;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// WPF implementation of the file dialog service
    /// </summary>
    public class FileDialogService : IFileDialogService
    {
        /// <summary>
        /// Show a file open dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public Task<string?> ShowOpenDialogAsync(string title = "Open File", string? initialDirectory = null, string? filter = null)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                InitialDirectory = initialDirectory ?? string.Empty,
                Filter = filter ?? "All files (*.*)|*.*"
            };

            var result = dialog.ShowDialog();
            return Task.FromResult(result == true ? dialog.FileName : null);
        }

        /// <summary>
        /// Show a file open dialog (synchronous version)
        /// </summary>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <returns>Selected file path or empty string if cancelled</returns>
        public string OpenFile(string filter, string title = "Open File", string? initialDirectory = null)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                InitialDirectory = initialDirectory ?? string.Empty,
                Filter = filter
            };

            var result = dialog.ShowDialog();
            return result == true ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// Show a file save dialog
        /// </summary>
        /// <param name="defaultFileName">Default file name</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="defaultExtension">Default file extension</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public Task<string?> ShowSaveDialogAsync(string defaultFileName, string? initialDirectory = null, string? defaultExtension = null, string? filter = null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = defaultFileName,
                InitialDirectory = initialDirectory ?? string.Empty,
                DefaultExt = defaultExtension ?? string.Empty,
                Filter = filter ?? "All files (*.*)|*.*"
            };

            var result = dialog.ShowDialog();
            return Task.FromResult(result == true ? dialog.FileName : null);
        }

        /// <summary>
        /// Show a folder browser dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <returns>Selected directory path or null if cancelled</returns>
        public Task<string?> ShowFolderBrowserDialogAsync(string title = "Select Folder", string? initialDirectory = null)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = title,
                SelectedPath = initialDirectory ?? string.Empty,
                ShowNewFolderButton = true
            };

            var result = dialog.ShowDialog();
            return Task.FromResult(result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null);
        }
    }
}
