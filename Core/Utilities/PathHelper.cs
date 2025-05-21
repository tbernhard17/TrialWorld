using System;
using System.IO;
using System.Linq;

namespace Core.Utilities
{
    /// <summary>
    /// Provides utilities for managing directories and file paths
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Ensures a directory exists, creating it if necessary
        /// </summary>
        /// <param name="path">Directory path to ensure exists</param>
        public static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Ensures the directory for a file exists, creating it if necessary
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>The original file path</returns>
        public static string EnsureDirectoryForFile(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            return filePath;
        }

        /// <summary>
        /// Creates a temporary directory with an optional base path
        /// </summary>
        /// <param name="basePath">Base path for the temporary directory (defaults to system temp path)</param>
        /// <returns>Path to the created temporary directory</returns>
        public static string CreateTempDirectory(string? basePath = null)
        {
            var tempDir = Path.Combine(
                basePath ?? Path.GetTempPath(),
                "TrialWorld_" + Path.GetRandomFileName());

            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        /// <summary>
        /// Safely combines path segments, handling null or empty segments
        /// </summary>
        /// <param name="paths">Path segments to combine</param>
        /// <returns>Combined path</returns>
        public static string CombinePaths(params string[] paths)
        {
            var filteredPaths = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();
            return filteredPaths.Length > 0 ? Path.Combine(filteredPaths) : string.Empty;
        }

        /// <summary>
        /// Safely deletes a directory and all its contents if it exists
        /// </summary>
        /// <param name="directoryPath">Path to the directory to delete</param>
        /// <param name="recursive">Whether to delete subdirectories and files</param>
        /// <returns>True if successful or if directory didn't exist, false if an error occurred</returns>
        public static bool SafeDeleteDirectory(string directoryPath, bool recursive = true)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, recursive);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
