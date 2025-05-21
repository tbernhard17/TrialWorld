using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace TrialWorld.Infrastructure.Utilities
{
    /// <summary>
    /// Provides utility methods for file system operations
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        /// Ensures a directory exists, creating it if necessary
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the directory exists or was created successfully, false otherwise</returns>
        public static bool EnsureDirectoryExists(string directoryPath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                logger?.LogError("Cannot ensure directory exists: directory path is null or empty");
                return false;
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    logger?.LogDebug("Creating directory: {DirectoryPath}", directoryPath);
                    Directory.CreateDirectory(directoryPath);
                }
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error creating directory: {DirectoryPath}", directoryPath);
                return false;
            }
        }

        /// <summary>
        /// Creates a directory if it doesn't exist, with retry logic
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="retryCount">Number of retry attempts</param>
        /// <param name="delayMs">Delay between retries in milliseconds</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the directory exists or was created successfully, false otherwise</returns>
        public static bool EnsureDirectoryExistsWithRetry(string directoryPath, int retryCount = 3, int delayMs = 500, ILogger? logger = null)
        {
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                if (EnsureDirectoryExists(directoryPath, logger))
                    return true;

                if (attempt < retryCount - 1)
                {
                    logger?.LogDebug("Retrying directory creation after {DelayMs}ms: {DirectoryPath}", delayMs, directoryPath);
                    Thread.Sleep(delayMs);
                }
            }

            return false;
        }

        /// <summary>
        /// Safely deletes a file if it exists
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the file was deleted successfully or didn't exist, false if an error occurred</returns>
        public static bool SafeDeleteFile(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot delete file: file path is null or empty");
                return false;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    logger?.LogDebug("Deleting file: {FilePath}", filePath);
                    File.Delete(filePath);
                }
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Safely copies a file, creating the destination directory if needed
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <param name="overwrite">Whether to overwrite the destination file if it exists</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the file was copied successfully, false otherwise</returns>
        public static bool SafeCopyFile(string sourcePath, string destinationPath, bool overwrite = false, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
            {
                logger?.LogError("Cannot copy file: source or destination path is null or empty");
                return false;
            }

            try
            {
                if (!File.Exists(sourcePath))
                {
                    logger?.LogError("Source file does not exist: {SourcePath}", sourcePath);
                    return false;
                }

                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    EnsureDirectoryExists(destinationDirectory, logger);
                }

                logger?.LogDebug("Copying file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                File.Copy(sourcePath, destinationPath, overwrite);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error copying file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                return false;
            }
        }

        /// <summary>
        /// Safely moves a file, creating the destination directory if needed
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <param name="overwrite">Whether to overwrite the destination file if it exists</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the file was moved successfully, false otherwise</returns>
        public static bool SafeMoveFile(string sourcePath, string destinationPath, bool overwrite = false, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
            {
                logger?.LogError("Cannot move file: source or destination path is null or empty");
                return false;
            }

            try
            {
                if (!File.Exists(sourcePath))
                {
                    logger?.LogError("Source file does not exist: {SourcePath}", sourcePath);
                    return false;
                }

                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    EnsureDirectoryExists(destinationDirectory, logger);
                }

                if (File.Exists(destinationPath))
                {
                    if (overwrite)
                    {
                        SafeDeleteFile(destinationPath, logger);
                    }
                    else
                    {
                        logger?.LogError("Destination file already exists: {DestinationPath}", destinationPath);
                        return false;
                    }
                }

                logger?.LogDebug("Moving file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error moving file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                return false;
            }
        }

        /// <summary>
        /// Gets the size of a file in bytes
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>Size of the file in bytes, or -1 if the file doesn't exist or an error occurred</returns>
        public static long GetFileSize(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot get file size: file path is null or empty");
                return -1;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return -1;
                }

                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error getting file size: {FilePath}", filePath);
                return -1;
            }
        }

        /// <summary>
        /// Checks if a file exists and is accessible
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the file exists and is accessible, false otherwise</returns>
        public static bool FileExistsAndAccessible(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot check file: file path is null or empty");
                return false;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Try to open the file to ensure it's accessible
                using (var stream = File.OpenRead(filePath))
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "File exists but is not accessible: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Gets files in a directory matching a search pattern
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="searchPattern">Search pattern (e.g., "*.mp4")</param>
        /// <param name="searchOption">Search option (TopDirectoryOnly or AllDirectories)</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>Array of file paths, or empty array if an error occurred</returns>
        public static string[] GetFiles(string directoryPath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                logger?.LogError("Cannot get files: directory path is null or empty");
                return Array.Empty<string>();
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    logger?.LogError("Directory does not exist: {DirectoryPath}", directoryPath);
                    return Array.Empty<string>();
                }

                return Directory.GetFiles(directoryPath, searchPattern, searchOption);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error getting files from directory: {DirectoryPath}", directoryPath);
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the last write time of a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>Last write time of the file, or null if the file doesn't exist or an error occurred</returns>
        public static DateTime? GetLastWriteTime(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot get last write time: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                return File.GetLastWriteTime(filePath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error getting last write time: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Safely reads all text from a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>File contents as string, or null if an error occurred</returns>
        public static string? SafeReadAllText(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot read file: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error reading file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Safely writes text to a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="contents">Text to write</param>
        /// <param name="createDirectory">Whether to create the directory if it doesn't exist</param>
        /// <param name="logger">Optional logger for operation logging</param>
        /// <returns>True if the file was written successfully, false otherwise</returns>
        public static bool SafeWriteAllText(string filePath, string contents, bool createDirectory = true, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot write file: file path is null or empty");
                return false;
            }

            try
            {
                if (createDirectory)
                {
                    var directoryPath = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        EnsureDirectoryExists(directoryPath, logger);
                    }
                }

                File.WriteAllText(filePath, contents);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                logger?.LogError(ex, "Error writing file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Gets a temporary file path in the system temporary directory
        /// </summary>
        /// <param name="prefix">Optional file name prefix</param>
        /// <param name="extension">Optional file extension (include the dot)</param>
        /// <returns>Path to a temporary file</returns>
        public static string GetTempFilePath(string? prefix = null, string? extension = null)
        {
            var fileName = string.IsNullOrEmpty(prefix)
                ? Guid.NewGuid().ToString()
                : $"{prefix}_{Guid.NewGuid()}";

            if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            return Path.Combine(Path.GetTempPath(), $"{fileName}{extension}");
        }
    }
}