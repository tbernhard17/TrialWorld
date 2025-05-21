using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrialWorld.Presentation.Interfaces;
using Microsoft.Win32;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// WPF specific implementation for IFileDialogService.
    /// </summary>
    public class WpfFileDialogService : IFileDialogService
    {
        private readonly ILogger<WpfFileDialogService> _logger;

        public WpfFileDialogService(ILogger<WpfFileDialogService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<string?> ShowOpenDialogAsync(string title = "Open File", string? initialDirectory = null, string? filter = null)
        {
            _logger.LogInformation("Showing Open File Dialog: Title='{Title}', Filter='{Filter}', InitialDir='{InitialDir}'", title, filter, initialDirectory);
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter ?? "All files|*.*",
                InitialDirectory = initialDirectory
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
            _logger.LogInformation("Showing Open File Dialog (Sync): Title='{Title}', Filter='{Filter}', InitialDir='{InitialDir}'", title, filter, initialDirectory);
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter,
                InitialDirectory = initialDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                _logger.LogInformation("File selected: {FileName}", dialog.FileName);
                return dialog.FileName;
            }
            else
            {
                _logger.LogInformation("Open File Dialog cancelled.");
                return string.Empty;
            }
        }

        public Task<string?> ShowSaveDialogAsync(string defaultFileName, string? initialDirectory = null, string? defaultExtension = null, string? filter = null)
        {
            _logger.LogInformation("Showing Save File Dialog: Title='Save File', Filter='{Filter}', DefaultFileName='{DefaultFileName}', InitialDir='{InitialDir}'", filter, defaultFileName, initialDirectory);
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save File",
                Filter = filter ?? "All files|*.*",
                FileName = defaultFileName,
                DefaultExt = defaultExtension,
                InitialDirectory = initialDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                _logger.LogInformation("Save location selected: {FileName}", dialog.FileName);
                return Task.FromResult<string?>(dialog.FileName);
            }
            else
            {
                _logger.LogInformation("Save File Dialog cancelled.");
                return Task.FromResult<string?>(null);
            }
        }

        public Task<string?> ShowFolderBrowserDialogAsync(string title = "Select Folder", string? initialDirectory = null)
        {
            _logger.LogInformation("Showing Folder Browser Dialog (using OpenFileDialog workaround): Title='{Title}', InitialDir='{InitialDir}'", title, initialDirectory);
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection.",
                Filter = "Folders|*.nonexistent",
                InitialDirectory = initialDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                string? folderPath = Path.GetDirectoryName(dialog.FileName);
                _logger.LogInformation("Folder selected: {FolderPath}", folderPath);
                return Task.FromResult<string?>(folderPath);
            }
            else
            {
                _logger.LogInformation("Folder Browser Dialog cancelled.");
                return Task.FromResult<string?>(null);
            }
        }
    }
}