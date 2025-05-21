using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    /// <summary>
    /// Implementation of IFileService that handles file operations
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ILoggingService _logger;
        
        /// <summary>
        /// Creates a new instance of FileService
        /// </summary>
        /// <param name="logger">Logging service</param>
        public FileService(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc/>
        public bool FileExists(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if file exists: {path}", ex);
                return false;
            }
        }
        
        /// <inheritdoc/>
        public string ReadAllText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading file: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading file asynchronously: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public void WriteAllText(string path, string contents)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(path, contents);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to file: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            // Default to UTF8 if no encoding specified
            await WriteAllTextAsync(path, contents, Encoding.UTF8, cancellationToken);
        }
        
        /// <inheritdoc/>
        public async Task WriteAllTextAsync(string path, string contents, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
                
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            await File.WriteAllTextAsync(path, contents, encoding ?? Encoding.UTF8, cancellationToken);
        }
        
        /// <inheritdoc/>
        public FileStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            try
            {
                return File.Open(path, mode, access);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error opening file: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.Copy(sourcePath, destinationPath, overwrite);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error copying file from {sourcePath} to {destinationPath}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public void MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
        {
            try
            {
                var directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                if (overwrite && File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                
                File.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error moving file from {sourcePath} to {destinationPath}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public FileInfo GetFileInfo(string path)
        {
            try
            {
                return new FileInfo(path);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting file info: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public void CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating directory: {path}", ex);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public string GetTempFilePath(string? extension = null)
        {
            try
            {
                var tempFileName = Path.GetTempFileName();
                
                if (!string.IsNullOrEmpty(extension))
                {
                    string formattedExtension = extension.StartsWith('.') ? extension : "." + extension;
                    var newTempFileName = Path.ChangeExtension(tempFileName, formattedExtension);
                    
                    if (File.Exists(newTempFileName))
                    {
                       File.Delete(newTempFileName);
                    }
                    
                    File.Move(tempFileName, newTempFileName);
                    return newTempFileName;
                }
                
                return tempFileName;
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error getting temporary file path", ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<string?> SelectFolderAsync(string? initialDirectory = null)
        {
            // This service is in Core and cannot show UI dialogs.
            // This method should ideally be implemented in a UI-layer service.
            _logger?.LogWarning("SelectFolderAsync is not implemented in the Core FileService.");
            throw new NotImplementedException("SelectFolderAsync cannot be called from Core FileService.");
        }

        /// <inheritdoc/>
        public Task<string[]?> SelectFilesAsync(string? initialDirectory = null)
        {
            // This service is in Core and cannot show UI dialogs.
            // This method should ideally be implemented in a UI-layer service.
             _logger?.LogWarning("SelectFilesAsync is not implemented in the Core FileService.");
            throw new NotImplementedException("SelectFilesAsync cannot be called from Core FileService.");
        }
    }
}
