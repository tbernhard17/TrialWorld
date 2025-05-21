using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TrialWorld.Infrastructure.Utilities
{
    /// <summary>
    /// Provides utilities for generating and comparing file hashes
    /// </summary>
    public static class FileHasher
    {
        /// <summary>
        /// Computes an MD5 hash for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>Hexadecimal MD5 hash string or null if the operation failed</returns>
        public static string? ComputeMd5Hash(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot compute hash: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                using var md5 = MD5.Create();
                using var stream = File.OpenRead(filePath);
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error computing MD5 hash for file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Computes an SHA256 hash for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>Hexadecimal SHA256 hash string or null if the operation failed</returns>
        public static string? ComputeSha256Hash(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot compute hash: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error computing SHA256 hash for file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Computes an SHA512 hash for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>Hexadecimal SHA512 hash string or null if the operation failed</returns>
        public static string? ComputeSha512Hash(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot compute hash: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                using var sha512 = SHA512.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha512.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error computing SHA512 hash for file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously computes an MD5 hash for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>Task with hexadecimal MD5 hash string or null if the operation failed</returns>
        public static async Task<string?> ComputeMd5HashAsync(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot compute hash: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                using var md5 = MD5.Create();
                using var stream = File.OpenRead(filePath);
                var hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error computing MD5 hash for file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously computes an SHA256 hash for a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>Task with hexadecimal SHA256 hash string or null if the operation failed</returns>
        public static async Task<string?> ComputeSha256HashAsync(string filePath, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot compute hash: file path is null or empty");
                return null;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath);
                    return null;
                }

                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = await sha256.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error computing SHA256 hash for file: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// Computes a hash for a string using the specified hashing algorithm
        /// </summary>
        /// <param name="input">Input string to hash</param>
        /// <param name="algorithm">Hashing algorithm to use (default: SHA256)</param>
        /// <returns>Hexadecimal hash string</returns>
        public static string ComputeStringHash(string input, HashAlgorithm? algorithm = null)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty", nameof(input));

            bool disposeAlgorithm = false;
            if (algorithm == null)
            {
                algorithm = SHA256.Create();
                disposeAlgorithm = true;
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = algorithm.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            finally
            {
                if (disposeAlgorithm)
                {
                    algorithm.Dispose();
                }
            }
        }

        /// <summary>
        /// Compares two files to determine if they are identical
        /// </summary>
        /// <param name="filePath1">Path to the first file</param>
        /// <param name="filePath2">Path to the second file</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>True if the files are identical, false otherwise</returns>
        public static bool AreFilesEqual(string filePath1, string filePath2, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath1) || string.IsNullOrEmpty(filePath2))
            {
                logger?.LogError("Cannot compare files: file path(s) is null or empty");
                return false;
            }

            try
            {
                if (!File.Exists(filePath1))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath1);
                    return false;
                }

                if (!File.Exists(filePath2))
                {
                    logger?.LogError("File does not exist: {FilePath}", filePath2);
                    return false;
                }

                var fileInfo1 = new FileInfo(filePath1);
                var fileInfo2 = new FileInfo(filePath2);

                // Quick check: if file sizes differ, files are not equal
                if (fileInfo1.Length != fileInfo2.Length)
                    return false;

                // Compute and compare hashes for a more reliable check
                string? hash1 = ComputeSha256Hash(filePath1, logger);
                string? hash2 = ComputeSha256Hash(filePath2, logger);

                return hash1 != null && hash2 != null && hash1 == hash2;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error comparing files: {FilePath1} and {FilePath2}", filePath1, filePath2);
                return false;
            }
        }

        /// <summary>
        /// Verifies a file against a known hash
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="expectedHash">Expected hash value</param>
        /// <param name="algorithm">Hash algorithm to use (default: SHA256)</param>
        /// <param name="logger">Optional logger for logging errors</param>
        /// <returns>True if the file hash matches the expected hash, false otherwise</returns>
        public static bool VerifyFileHash(string filePath, string expectedHash, 
            HashAlgorithmType algorithm = HashAlgorithmType.SHA256, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot verify hash: file path is null or empty");
                return false;
            }

            if (string.IsNullOrEmpty(expectedHash))
            {
                logger?.LogError("Cannot verify hash: expected hash is null or empty");
                return false;
            }

            try
            {
                string? actualHash = null;
                
                switch (algorithm)
                {
                    case HashAlgorithmType.MD5:
                        actualHash = ComputeMd5Hash(filePath, logger);
                        break;
                    case HashAlgorithmType.SHA256:
                        actualHash = ComputeSha256Hash(filePath, logger);
                        break;
                    case HashAlgorithmType.SHA512:
                        actualHash = ComputeSha512Hash(filePath, logger);
                        break;
                    default:
                        throw new ArgumentException("Unsupported hash algorithm type", nameof(algorithm));
                }

                if (actualHash == null)
                    return false;

                // Compare hashes case-insensitively
                return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                logger?.LogError(ex, "Error verifying hash for file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Supported hash algorithm types
        /// </summary>
        public enum HashAlgorithmType
        {
            MD5,
            SHA256,
            SHA512
        }
    }
}
