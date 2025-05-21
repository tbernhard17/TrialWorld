using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Infrastructure.Services
{
    public class TranscriptionVerificationService : ITranscriptionVerificationService
    {
        private readonly ILogger<TranscriptionVerificationService> _logger;
        private readonly IDatabaseLoaderService _databaseLoader;
        private readonly TranscriptionPathSettings _settings;
        private readonly string _verificationDbPath;
        
        public TranscriptionVerificationService(
            ILogger<TranscriptionVerificationService> logger,
            IDatabaseLoaderService databaseLoader,
            IOptions<TranscriptionPathSettings> settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseLoader = databaseLoader ?? throw new ArgumentNullException(nameof(databaseLoader));
            _settings = settings?.Value ?? new TranscriptionPathSettings();
            
            // Create a dedicated path for tracking transcription verification
            _verificationDbPath = Path.Combine(_settings.TranscriptionDatabasePath, "verification");
            if (!Directory.Exists(_verificationDbPath))
            {
                Directory.CreateDirectory(_verificationDbPath);
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> IsAlreadyTranscribedAsync(string filePath, string fileHash)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            
            try
            {
                string fileName = Path.GetFileName(filePath);
                
                // First check for matching file hash
                if (!string.IsNullOrEmpty(fileHash))
                {
                    var hashDbPath = Path.Combine(_verificationDbPath, $"{fileHash}.json");
                    if (File.Exists(hashDbPath))
                    {
                        // Read the file to check if the transcription was successful
                        string json = await File.ReadAllTextAsync(hashDbPath);
                        var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        
                        // Check if the status indicates a successful transcription
                        if (root.TryGetProperty("Status", out var statusProp))
                        {
                            string status = statusProp.GetString() ?? string.Empty;
                            bool isVerified = root.TryGetProperty("IsVerified", out var verifiedProp) && verifiedProp.GetBoolean();
                            
                            if (status == "Completed" && isVerified)
                            {
                                _logger.LogInformation("Found existing successful transcription by file hash for {FileName}", fileName);
                                return true;
                            }
                            
                            _logger.LogWarning("Found existing transcription record for {FileName} but status is {Status} and verification is {IsVerified}",
                                fileName, status, isVerified);
                            return false;
                        }
                        
                        // If no status property, we need to check the file itself
                        _logger.LogWarning("Found existing transcription record for {FileName} but it lacks status information", fileName);
                    }
                }
                
                // Next, check if output file already exists
                string outputPath = GetExpectedOutputPath(filePath);
                if (File.Exists(outputPath))
                {
                    // Verify the existing file is a complete and valid transcription
                    if (await VerifyTranscriptionFile(outputPath))
                    {
                        _logger.LogInformation("Found existing complete transcription file for {FileName}", fileName);
                        return true;
                    }
                    
                    _logger.LogWarning("Found an incomplete transcription file for {FileName} - will transcribe again", fileName);
                    // File exists but is incomplete - we'll need to retranscribe
                    return false;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for existing transcription for {FilePath}", filePath);
                // If we can't verify, assume it's not transcribed to be safe
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> VerifyTranscriptionCompletionAsync(string transcriptionFilePath)
        {
            if (string.IsNullOrEmpty(transcriptionFilePath)) 
                throw new ArgumentNullException(nameof(transcriptionFilePath));
            
            try
            {
                if (!File.Exists(transcriptionFilePath))
                {
                    _logger.LogWarning("Transcription file doesn't exist: {FilePath}", transcriptionFilePath);
                    return false;
                }
                
                return await VerifyTranscriptionFile(transcriptionFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying transcription completion for {FilePath}", transcriptionFilePath);
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task RegisterTranscriptionAsync(string filePath, string fileHash, string transcriptionId, string outputPath, string status)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            
            try
            {
                // Store file hash for future duplicate detection
                if (!string.IsNullOrEmpty(fileHash))
                {
                    var verificationEntry = new
                    {
                        FileHash = fileHash,
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath,
                        TranscriptionId = transcriptionId ?? string.Empty,
                        OutputPath = outputPath ?? string.Empty,
                        LastAttempt = DateTime.Now,
                        AttemptCount = 1,
                        Status = status ?? "Processing"
                    };
                    
                    var hashDbPath = Path.Combine(_verificationDbPath, $"{fileHash}.json");
                    await File.WriteAllTextAsync(hashDbPath, JsonSerializer.Serialize(verificationEntry, new JsonSerializerOptions { WriteIndented = true }));
                    
                    _logger.LogInformation("Registered transcription for {FileName} with hash {FileHash}", Path.GetFileName(filePath), fileHash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering transcription for {FilePath}", filePath);
                // Continue even if registration fails - non-critical operation
            }
        }
        
        /// <inheritdoc/>
        public async Task UpdateTranscriptionStatusAsync(string fileHash, string transcriptionId, string status, bool isVerified)
        {
            try
            {
                if (string.IsNullOrEmpty(fileHash))
                {
                    return;
                }
                
                var hashDbPath = Path.Combine(_verificationDbPath, $"{fileHash}.json");
                if (!File.Exists(hashDbPath))
                {
                    return;
                }
                
                // Read the existing entry to preserve file information
                string json = await File.ReadAllTextAsync(hashDbPath);
                var doc = JsonDocument.Parse(json);
                var rootElement = doc.RootElement;
                
                // Extract existing values
                string fileName = rootElement.TryGetProperty("FileName", out var fileNameProp) 
                    ? fileNameProp.GetString() ?? "unknown" 
                    : "unknown";
                    
                string filePath = rootElement.TryGetProperty("FilePath", out var filePathProp) 
                    ? filePathProp.GetString() ?? "" 
                    : "";
                    
                string outputPath = rootElement.TryGetProperty("OutputPath", out var outputPathProp) 
                    ? outputPathProp.GetString() ?? "" 
                    : "";
                    
                int attemptCount = rootElement.TryGetProperty("AttemptCount", out var attemptProp) 
                    ? attemptProp.GetInt32() 
                    : 1;
                
                // Create updated entry
                var verificationEntry = new
                {
                    FileHash = fileHash,
                    FileName = fileName,
                    FilePath = filePath,
                    TranscriptionId = transcriptionId,
                    OutputPath = outputPath,
                    LastAttempt = DateTime.Now,
                    AttemptCount = attemptCount,
                    Status = status,
                    IsVerified = isVerified
                };
                
                await File.WriteAllTextAsync(hashDbPath, JsonSerializer.Serialize(verificationEntry, new JsonSerializerOptions { WriteIndented = true }));
                
                _logger.LogInformation("Updated status for {FileName} to {Status}", fileName, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transcription status for hash {FileHash}", fileHash);
                // Continue even if update fails - non-critical operation
            }
        }
        
        /// <inheritdoc/>
        public string GetExpectedOutputPath(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentNullException(nameof(inputFilePath));
                
            var fileName = Path.GetFileNameWithoutExtension(inputFilePath);
            return Path.Combine(_settings.TranscriptionDatabasePath, $"{fileName}.json");
        }
        
        /// <summary>
        /// Verifies if a transcription file is complete and valid
        /// </summary>
        /// <param name="filePath">Path to the transcription file</param>
        /// <returns>True if the file is a valid and complete transcription; otherwise false</returns>
        private async Task<bool> VerifyTranscriptionFile(string filePath)
        {
            try
            {
                // Basic file check
                if (!File.Exists(filePath)) return false;
                
                // Check file size - empty or too small files aren't valid
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length < 100) // Arbitrary min size 
                {
                    _logger.LogWarning("Transcription file too small: {Size} bytes", fileInfo.Length);
                    return false;
                }
                
                // Try to read as JSON
                var fileContent = await File.ReadAllTextAsync(filePath);
                using var document = JsonDocument.Parse(fileContent);
                
                // Check for required transcription properties
                var rootElement = document.RootElement;
                
                // Check for AssemblyAI format
                if (rootElement.TryGetProperty("status", out var statusProp))
                {
                    var statusValue = statusProp.GetString();
                    // Only "completed" status is considered valid
                    if (statusValue != "completed")
                    {
                        _logger.LogWarning("Transcription file has non-completed status: {Status}", statusValue);
                        return false;
                    }
                    
                    // Check if it has text content
                    if (rootElement.TryGetProperty("text", out var textProp))
                    {
                        var text = textProp.GetString();
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            _logger.LogWarning("Transcription file has empty text content");
                            return false;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Transcription file missing text property");
                        return false;
                    }
                    
                    // Verify it has utterances and words (speech segments)
                    bool hasUtterances = rootElement.TryGetProperty("utterances", out var _);
                    bool hasWords = rootElement.TryGetProperty("words", out var wordsArray) && wordsArray.GetArrayLength() > 0;
                    
                    if (!hasUtterances || !hasWords)
                    {
                        _logger.LogWarning("Transcription file missing speech segments");
                        return false;
                    }
                    
                    return true;
                }
                
                // Otherwise, check for our own converted format
                if (rootElement.TryGetProperty("segments", out var segmentsProp) && 
                    segmentsProp.ValueKind == JsonValueKind.Array && 
                    segmentsProp.GetArrayLength() > 0)
                {
                    return true;
                }
                
                _logger.LogWarning("Transcription file has invalid format");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying transcription file {FilePath}", filePath);
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> CalculateFileHashAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
            }

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating file hash for {FilePath}: {Message}", filePath, ex.Message);
                throw new IOException($"Error calculating file hash: {ex.Message}", ex);
            }
        }
    }
}
