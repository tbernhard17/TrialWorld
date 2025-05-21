using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for a service that handles file operations
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Checks if a file exists at the specified path
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>True if the file exists, false otherwise</returns>
        bool FileExists(string path);
        
        /// <summary>
        /// Reads all text from a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>The contents of the file as a string</returns>
        string ReadAllText(string path);
        
        /// <summary>
        /// Asynchronously reads all text from a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task containing the contents of the file as a string</returns>
        Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Writes text to a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="contents">Text to write</param>
        void WriteAllText(string path, string contents);
        
        /// <summary>
        /// Asynchronously writes text to a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="contents">Text to write</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the write operation</returns>
        Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a stream for reading from a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="mode">File mode (Open, Create, etc.)</param>
        /// <param name="access">File access (Read, Write, etc.)</param>
        /// <returns>A FileStream for the specified file</returns>
        FileStream OpenFile(string path, FileMode mode, FileAccess access);
        
        /// <summary>
        /// Copies a file to a new location
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        /// <param name="overwrite">Whether to overwrite if the destination file exists</param>
        void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
        
        /// <summary>
        /// Moves a file to a new location
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        /// <param name="overwrite">Whether to overwrite if the destination file exists</param>
        void MoveFile(string sourcePath, string destinationPath, bool overwrite = false);
        
        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        void DeleteFile(string path);
        
        /// <summary>
        /// Gets information about a file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>FileInfo object for the specified file</returns>
        FileInfo GetFileInfo(string path);
        
        /// <summary>
        /// Creates a directory if it doesn't exist
        /// </summary>
        /// <param name="path">Path to the directory</param>
        void CreateDirectoryIfNotExists(string path);
        
        /// <summary>
        /// Gets a temporary file path
        /// </summary>
        /// <param name="extension">Optional file extension</param>
        /// <returns>Path to a temporary file</returns>
        string GetTempFilePath(string? extension = null);
        
        /// <summary>
        /// Selects a folder using the folder open dialog.
        /// </summary>
        /// <param name="initialDirectory">The initial directory to open the dialog in.</param>
        /// <returns>The selected folder path or null if canceled.</returns>
        Task<string?> SelectFolderAsync(string? initialDirectory = null);
        
        /// <summary>
        /// Selects multiple files using the file open dialog.
        /// </summary>
        /// <param name="initialDirectory">The initial directory to open the dialog in.</param>
        /// <returns>The selected file paths or null if canceled.</returns>
        Task<string[]?> SelectFilesAsync(string? initialDirectory = null);
    }
}
