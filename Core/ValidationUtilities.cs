using System;
using System.IO;

namespace Core
{
    /// <summary>
    /// Provides utilities for validating file paths and related resources
    /// </summary>
    public static class FileValidation
    {
        /// <summary>
        /// Validates that a file path is not null or empty and that the file exists
        /// </summary>
        /// <param name="filePath">Path to validate</param>
        /// <param name="paramName">Parameter name for the exception</param>
        /// <exception cref="ArgumentException">Thrown if the file path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist</exception>
        public static void ValidateFilePath(string filePath, string paramName = "filePath")
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", paramName);
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }
        
        /// <summary>
        /// Validates that a directory path is not null or empty
        /// </summary>
        /// <param name="directoryPath">Directory path to validate</param>
        /// <param name="paramName">Parameter name for the exception</param>
        /// <exception cref="ArgumentException">Thrown if the directory path is null or empty</exception>
        public static void ValidateDirectory(string directoryPath, string paramName = "directoryPath")
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty", paramName);
        }
        
        /// <summary>
        /// Validates that a file exists and has a supported media extension
        /// </summary>
        /// <param name="filePath">Path to the media file</param>
        /// <returns>True if the file exists and has a supported extension, false otherwise</returns>
        public static bool IsValidMediaFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
                
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return extension is ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".mp3" or ".wav" or ".aac";
        }
        
        /// <summary>
        /// Checks if a file path has a supported video extension
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if the file has a supported video extension</returns>
        public static bool IsVideoFile(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return extension is ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv";
        }
        
        /// <summary>
        /// Checks if a file path has a supported audio extension
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if the file has a supported audio extension</returns>
        public static bool IsAudioFile(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return extension is ".mp3" or ".wav" or ".aac" or ".m4a" or ".flac";
        }

        /// <summary>
        /// Ensures that the specified directory exists. If it doesn't exist, it attempts to create it.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to ensure exists.</param>
        /// <exception cref="ArgumentException">Thrown if the directory path is null, empty, or invalid.</exception>
        /// <exception cref="IOException">Thrown if an I/O error occurred during directory creation.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the caller does not have the required permission to create the directory.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified path is invalid (for example, it is on an unmapped drive).</exception>
        public static void EnsureOutputDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Directory path cannot be null or whitespace.", nameof(directoryPath));
            }

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    // Attempt to create the directory
                    Directory.CreateDirectory(directoryPath);
                     // Log or confirm creation if necessary
                    // Console.WriteLine($"Created directory: {directoryPath}"); 
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is PathTooLongException || ex is DirectoryNotFoundException || ex is NotSupportedException)
            {
                // Log the specific exception details if needed
                // Console.WriteLine($"Error ensuring directory exists: {ex.Message}");
                // Re-throw the original exception to maintain specific error information
                throw;
            }
            catch (Exception ex) // Catch any other unexpected exceptions
            {
                 // Log unexpected errors
                 // Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                 // Wrap in a more generic exception or re-throw depending on error handling strategy
                 throw new IOException($"An unexpected error occurred while ensuring directory '{directoryPath}' exists.", ex);
            }
        }
    }
}