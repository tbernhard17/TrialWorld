using System;
using System.Threading.Tasks;

namespace TrialWorld.Core.Interfaces
{
    /// <summary>
    /// Service for verifying and preventing duplicate transcriptions
    /// </summary>
    public interface ITranscriptionVerificationService
    {
        /// <summary>
        /// Checks if a file has already been transcribed by examining database records
        /// </summary>
        /// <param name="filePath">Path to the media file</param>
        /// <param name="fileHash">Hash of the file content (if available)</param>
        /// <returns>True if the file has already been transcribed; otherwise false</returns>
        Task<bool> IsAlreadyTranscribedAsync(string filePath, string fileHash);

        /// <summary>
        /// Verifies that a completed transcription is fully processed and valid
        /// </summary>
        /// <param name="transcriptionFilePath">Path to the transcription file</param>
        /// <returns>True if transcription is complete and valid; otherwise false</returns>
        Task<bool> VerifyTranscriptionCompletionAsync(string transcriptionFilePath);
        
        /// <summary>
        /// Registers a transcription in the database to prevent duplicate processing
        /// </summary>
        /// <param name="filePath">Path to the media file</param>
        /// <param name="fileHash">Hash of the file content</param>
        /// <param name="transcriptionId">ID of the transcription</param>
        /// <param name="outputPath">Path to the output transcription file</param>
        /// <param name="status">Current status of the transcription</param>
        Task RegisterTranscriptionAsync(string filePath, string fileHash, string transcriptionId, string outputPath, string status);
        
        /// <summary>
        /// Updates the status of an existing transcription record
        /// </summary>
        /// <param name="fileHash">Hash of the file content</param>
        /// <param name="transcriptionId">ID of the transcription</param>
        /// <param name="status">New status of the transcription</param>
        /// <param name="isVerified">Whether the transcription has been verified</param>
        Task UpdateTranscriptionStatusAsync(string fileHash, string transcriptionId, string status, bool isVerified);
        
        /// <summary>
        /// Generates the expected output path for a transcription based on input file
        /// </summary>
        /// <param name="inputFilePath">Path to the input media file</param>
        /// <returns>Expected path for the output transcription file</returns>
        string GetExpectedOutputPath(string inputFilePath);
        
        /// <summary>
        /// Calculates a hash for the specified file to uniquely identify it
        /// </summary>
        /// <param name="filePath">Path to the file to hash</param>
        /// <returns>A string hash representation of the file contents</returns>
        Task<string> CalculateFileHashAsync(string filePath);
    }
}
