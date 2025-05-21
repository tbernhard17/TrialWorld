using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Core.Utilities
{
    /// <summary>
    /// Provides utility methods for working with file and directory paths
    /// </summary>
    public static class PathUtilities
    {
        /// <summary>
        /// Gets the common base path shared by multiple paths
        /// </summary>
        /// <param name="paths">Collection of paths to analyze</param>
        /// <returns>Common base path or empty string if no common path exists</returns>
        public static string GetCommonBasePath(IEnumerable<string> paths)
        {
            if (paths == null || !paths.Any())
                return string.Empty;

            string[] normalizedPaths = paths
                .Select(p => Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                .ToArray();

            if (normalizedPaths.Length == 1)
                return Path.GetDirectoryName(normalizedPaths[0]) ?? string.Empty;

            string firstPath = normalizedPaths[0];
            int commonPrefixLength = firstPath.Length;

            foreach (string path in normalizedPaths.Skip(1))
            {
                commonPrefixLength = Math.Min(commonPrefixLength, path.Length);
                for (int i = 0; i < commonPrefixLength; i++)
                {
                    if (firstPath[i] != path[i])
                    {
                        commonPrefixLength = i;
                        break;
                    }
                }
            }

            // Find the last directory separator in the common prefix
            if (commonPrefixLength == 0)
                return string.Empty;

            int lastSeparatorPos = firstPath.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, commonPrefixLength - 1);
            return lastSeparatorPos > 0 ? firstPath.Substring(0, lastSeparatorPos) : string.Empty;
        }

        /// <summary>
        /// Sanitizes a file name by removing invalid characters
        /// </summary>
        /// <param name="fileName">File name to sanitize</param>
        /// <param name="replacementChar">Character to replace invalid characters with</param>
        /// <returns>Sanitized file name</returns>
        public static string SanitizeFileName(string fileName, char replacementChar = '_')
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Select(c => invalidChars.Contains(c) ? replacementChar : c).ToArray());
        }

        /// <summary>
        /// Makes a path relative to a specified base path
        /// </summary>
        /// <param name="path">Absolute path to convert</param>
        /// <param name="basePath">Base path to make the path relative to</param>
        /// <returns>Relative path</returns>
        public static string MakeRelativePath(string path, string basePath)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentException("Base path cannot be null or empty", nameof(basePath));

            // Ensure trailing directory separator for base path
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) && 
                !basePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                basePath += Path.DirectorySeparatorChar;
            }

            Uri baseUri = new Uri(basePath);
            Uri pathUri = new Uri(path);
            
            if (baseUri.Scheme != pathUri.Scheme)
                return path;

            Uri relativeUri = baseUri.MakeRelativeUri(pathUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            
            // Convert directory separators to platform-specific ones
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Ensures a directory exists, creates it if it doesn't
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <returns>True if the directory exists or was created, false otherwise</returns>
        public static bool EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return false;

            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a unique file name in a directory by appending a number if the file already exists
        /// </summary>
        /// <param name="filePath">Original file path</param>
        /// <returns>Unique file path</returns>
        public static string GetUniqueFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                return filePath;

            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int counter = 1;

            string newFilePath;
            do
            {
                newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension} ({counter}){extension}");
                counter++;
            } while (File.Exists(newFilePath));

            return newFilePath;
        }

        /// <summary>
        /// Checks if a path is a valid file path
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the path is a valid file path, false otherwise</returns>
        public static bool IsValidFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                // Check if the path is too long
                if (path.Length >= 260)
                    return false;

                // Get the file name from the path
                string fileName = Path.GetFileName(path);
                if (string.IsNullOrEmpty(fileName))
                    return false;

                // Check if the file name contains any invalid characters
                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (fileName.IndexOfAny(invalidChars) >= 0)
                    return false;

                // Try to get full path to check validity of path format
                string fullPath = Path.GetFullPath(path);
                
                // Make sure we're not just a directory
                return !string.IsNullOrEmpty(Path.GetFileName(fullPath));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all files in a directory matching specified patterns
        /// </summary>
        /// <param name="directory">Directory to search</param>
        /// <param name="patterns">File patterns to match (e.g., "*.jpg", "*.png")</param>
        /// <param name="searchOption">Specifies whether to search subdirectories</param>
        /// <returns>Collection of matching file paths</returns>
        public static IEnumerable<string> GetFilesWithPatterns(string directory, string[] patterns, 
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Directory cannot be null or empty", nameof(directory));

            if (patterns == null || patterns.Length == 0)
                throw new ArgumentException("At least one pattern must be specified", nameof(patterns));

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory not found: {directory}");

            return patterns
                .SelectMany(pattern => Directory.GetFiles(directory, pattern, searchOption))
                .Distinct();
        }

        /// <summary>
        /// Gets the size of a directory in bytes
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="includeSubdirectories">Whether to include subdirectories in the calculation</param>
        /// <returns>Size of the directory in bytes</returns>
        public static long GetDirectorySize(string directoryPath, bool includeSubdirectories = true)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directoryPath, "*", searchOption)
                .Sum(file => new FileInfo(file).Length);
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable string (e.g., "1.5 MB")
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <param name="decimals">Number of decimal places to round to</param>
        /// <returns>Formatted file size string</returns>
        public static string FormatFileSize(long bytes, int decimals = 2)
        {
            if (bytes < 0)
                throw new ArgumentException("Bytes cannot be negative", nameof(bytes));

            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int suffixIndex = 0;
            double formattedSize = bytes;

            while (formattedSize >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                formattedSize /= 1024;
                suffixIndex++;
            }

            return $"{Math.Round(formattedSize, decimals)} {suffixes[suffixIndex]}";
        }
    }
}
