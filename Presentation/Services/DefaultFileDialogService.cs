using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrialWorld.Presentation.Interfaces;

namespace TrialWorld.Presentation.Services
{
    /// <summary>
    /// Default implementation of file dialog service that uses console input for non-UI environments
    /// </summary>
    public class DefaultFileDialogService : IFileDialogService
    {
        private readonly ILogger<DefaultFileDialogService> _logger;

        public DefaultFileDialogService(ILogger<DefaultFileDialogService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Show a file open dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public Task<string?> ShowOpenDialogAsync(
            string title = "Open File",
            string? initialDirectory = null,
            string? filter = null)
        {
            _logger.LogInformation("Opening file dialog with title: {Title}", title);
            
            try
            {
                // Use the synchronous implementation and wrap in Task
                string result = OpenFile(filter ?? "All files|*.*", title, initialDirectory);
                return Task.FromResult<string?>(string.IsNullOrEmpty(result) ? null : result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing open file dialog");
                return Task.FromResult<string?>(null);
            }
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
            try
            {
                Console.WriteLine(title);
                Console.WriteLine($"Initial directory: {initialDirectory ?? Directory.GetCurrentDirectory()}");

                if (!string.IsNullOrEmpty(filter))
                {
                    Console.WriteLine($"Filter: {filter}");
                }

                Console.WriteLine("Enter file path (or leave empty to cancel):");
                var path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return string.Empty;
                }

                // Validate file existence
                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path}");
                    return string.Empty;
                }

                return path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing open file dialog: {Message}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Show a file save dialog
        /// </summary>
        /// <param name="defaultFileName">Default file name</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <param name="defaultExtension">Default file extension</param>
        /// <param name="filter">File filter (e.g., "Text files|*.txt|All files|*.*")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public Task<string?> ShowSaveDialogAsync(
            string defaultFileName,
            string? initialDirectory = null,
            string? defaultExtension = null,
            string? filter = null)
        {
            try
            {
                Console.WriteLine("Save File");
                Console.WriteLine($"Initial directory: {initialDirectory ?? Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Default file name: {defaultFileName}");

                if (!string.IsNullOrEmpty(filter))
                {
                    Console.WriteLine($"Filter: {filter}");
                }

                var defaultPath = Path.Combine(initialDirectory ?? Directory.GetCurrentDirectory(), defaultFileName);
                Console.WriteLine($"Default path: {defaultPath}");
                Console.WriteLine("Enter file path (or leave empty to cancel):");
                var path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return Task.FromResult<string?>(null);
                }

                // Add default extension if needed
                if (!string.IsNullOrEmpty(defaultExtension) && !Path.HasExtension(path))
                {
                    path = path + (defaultExtension.StartsWith(".") ? defaultExtension : "." + defaultExtension);
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return Task.FromResult((string?)path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing save file dialog: {Message}", ex.Message);
                return Task.FromResult<string?>(null);
            }
        }

        /// <summary>
        /// Show a folder browser dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="initialDirectory">Initial directory to show</param>
        /// <returns>Selected directory path or null if cancelled</returns>
        public Task<string?> ShowFolderBrowserDialogAsync(
            string title = "Select Folder",
            string? initialDirectory = null)
        {
            try
            {
                Console.WriteLine(title);
                Console.WriteLine($"Initial directory: {initialDirectory ?? Directory.GetCurrentDirectory()}");
                Console.WriteLine("Enter folder path (or leave empty to cancel):");
                var path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return Task.FromResult<string?>(null);
                }

                // Create directory if it doesn't exist
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Directory does not exist. Create it? (y/n)");
                    var response = Console.ReadLine()?.ToLower();

                    if (response == "y" || response == "yes")
                    {
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        return Task.FromResult<string?>(null);
                    }
                }

                return Task.FromResult((string?)path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing folder browser dialog: {Message}", ex.Message);
                return Task.FromResult<string?>(null);
            }
        }
    }
}