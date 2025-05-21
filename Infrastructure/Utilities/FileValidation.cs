using System;
using System.IO;

namespace TrialWorld.Infrastructure.Utilities
{
    /// <summary>
    /// Provides basic file and directory validation helpers.
    /// </summary>
    internal static class FileValidation
    {
        /// <summary>
        /// Validates a file path, ensuring it's not null/empty and the file exists.
        /// Throws exceptions if validation fails.
        /// </summary>
        public static void ValidateFilePath(string? filePath, string paramName = "filePath")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", paramName);
            }

            if (!File.Exists(filePath))
            {
                // Consider logging the error before throwing
                // _logger.LogError("Input file not found: {FilePath}", filePath);
                throw new FileNotFoundException("Input file not found.", filePath);
            }
        }

        /// <summary>
        /// Ensures the specified directory exists, creating it if necessary.
        /// Throws exceptions if creation fails.
        /// </summary>
        public static void EnsureOutputDirectory(string? directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                // Or throw ArgumentNullException/ArgumentException depending on desired behavior
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    // Consider logging success: _logger.LogDebug("Created directory: {DirectoryPath}", directoryPath);
                }
                catch (Exception ex)
                {
                    // Consider logging error: _logger.LogError(ex, "Failed to create directory: {DirectoryPath}", directoryPath);
                    throw new IOException($"Failed to create directory: {directoryPath}", ex);
                }
            }
        }
    }
}