using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Infrastructure.Transcription.Configuration;
using TrialWorld.Infrastructure.Transcription.DTOs;

namespace TrialWorld.Infrastructure.Transcription.Services
{
    /// <summary>
    /// Implementation of direct integration with the AssemblyAI REST API.
    /// </summary>
    public class AssemblyAIDirectApiService : IAssemblyAIDirectApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AssemblyAIDirectApiService> _logger;
        private readonly AssemblyAIOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string UploadEndpoint = "/upload";
        private const string TranscriptEndpoint = "/transcript";

        /// <summary>
        /// Initializes a new instance of the AssemblyAIDirectApiService class.
        /// </summary>
        /// <param name="httpClient">The HTTP client (pre-configured by HttpClientFactory).</param>
        /// <param name="options">The AssemblyAI options.</param>
        /// <param name="logger">The logger.</param>
        public AssemblyAIDirectApiService(
            HttpClient httpClient,
            IOptions<AssemblyAIOptions> options,
            ILogger<AssemblyAIDirectApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // HttpClient is now configured in TranscriptionServiceRegistration.cs
            // using the HttpClientFactory pattern for better lifetime management

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            _logger.LogInformation("Uploading file to AssemblyAI");
            
            try
            {
                using var content = new StreamContent(fileStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                
                var response = await _httpClient.PostAsync(UploadEndpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var uploadResponse = await response.Content.ReadFromJsonAsync<UploadResponseDto>(_jsonOptions, cancellationToken);
                
                if (uploadResponse == null || string.IsNullOrEmpty(uploadResponse.UploadUrl))
                {
                    throw new InvalidOperationException("Failed to get upload URL from AssemblyAI");
                }
                
                _logger.LogInformation("File uploaded successfully to AssemblyAI");
                return uploadResponse.UploadUrl;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while uploading file to AssemblyAI");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file to AssemblyAI");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> SubmitTranscriptionAsync(TranscriptionRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.AudioUrl))
                throw new ArgumentException("AudioUrl cannot be null or empty", nameof(request));

            _logger.LogInformation("Submitting transcription request to AssemblyAI");
            
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(TranscriptEndpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var transcriptionResponse = await response.Content.ReadFromJsonAsync<TranscriptionResponseDto>(_jsonOptions, cancellationToken);
                
                if (transcriptionResponse == null || string.IsNullOrEmpty(transcriptionResponse.Id))
                {
                    throw new InvalidOperationException("Failed to get transcription ID from AssemblyAI");
                }
                
                _logger.LogInformation("Transcription request submitted successfully to AssemblyAI. ID: {TranscriptionId}", transcriptionResponse.Id);
                return transcriptionResponse.Id;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while submitting transcription request to AssemblyAI");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting transcription request to AssemblyAI");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResponseDto> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
                throw new ArgumentException("TranscriptionId cannot be null or empty", nameof(transcriptionId));

            _logger.LogDebug("Getting transcription status from AssemblyAI. ID: {TranscriptionId}", transcriptionId);
            
            try
            {
                var response = await _httpClient.GetAsync($"{TranscriptEndpoint}/{transcriptionId}", cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var transcriptionResponse = await response.Content.ReadFromJsonAsync<TranscriptionResponseDto>(_jsonOptions, cancellationToken);
                
                if (transcriptionResponse == null)
                {
                    throw new InvalidOperationException("Failed to get transcription status from AssemblyAI");
                }
                
                _logger.LogDebug("Got transcription status from AssemblyAI. ID: {TranscriptionId}, Status: {Status}", 
                    transcriptionId, transcriptionResponse.Status);
                
                return transcriptionResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while getting transcription status from AssemblyAI. ID: {TranscriptionId}", transcriptionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting transcription status from AssemblyAI. ID: {TranscriptionId}", transcriptionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TranscriptionResponseDto> PollForCompletionAsync(string transcriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(transcriptionId))
                throw new ArgumentException("TranscriptionId cannot be null or empty", nameof(transcriptionId));

            _logger.LogInformation("Polling for transcription completion from AssemblyAI. ID: {TranscriptionId}", transcriptionId);
            
            int attempts = 0;
            const int maxAttempts = 60; // 5 minutes with 5-second intervals
            
            while (attempts < maxAttempts)
            {
                attempts++;
                
                try
                {
                    var response = await GetTranscriptionStatusAsync(transcriptionId, cancellationToken);
                    
                    if (response.Status == "completed" || response.Status == "error")
                    {
                        _logger.LogInformation("Transcription completed with status: {Status}. ID: {TranscriptionId}", 
                            response.Status, transcriptionId);
                        return response;
                    }
                    
                    _logger.LogDebug("Transcription still in progress. Status: {Status}. Attempt: {Attempt}/{MaxAttempts}", 
                        response.Status, attempts, maxAttempts);
                    
                    await Task.Delay(_options.PollingIntervalMs, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Error occurred while polling for transcription completion. Attempt: {Attempt}/{MaxAttempts}", 
                        attempts, maxAttempts);
                    
                    // Implement exponential backoff
                    await Task.Delay(Math.Min(_options.PollingIntervalMs * attempts, 30000), cancellationToken);
                }
            }
            
            throw new TimeoutException($"Polling for transcription completion timed out after {maxAttempts} attempts");
        }

        /// <inheritdoc/>
        public async Task<MediaTranscript> TranscribeFileAsync(
            string filePath, 
            TranscriptionRequestDto? options = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("FilePath cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Audio file not found", filePath);

            _logger.LogInformation("Starting transcription process for file: {FilePath}", filePath);
            
            try
            {
                // Step 1: Upload the file
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var uploadUrl = await UploadFileAsync(fileStream, cancellationToken);
                
                // Step 2: Submit transcription request
                var request = options ?? new TranscriptionRequestDto
                {
                    AudioUrl = uploadUrl,
                    LanguageCode = "en_us",
                    SpeakerLabels = true,
                    SentimentAnalysis = true
                };
                
                // Make sure the audio URL is set
                request.AudioUrl = uploadUrl;
                
                var transcriptionId = await SubmitTranscriptionAsync(request, cancellationToken);
                
                // Step 3: Poll for completion
                var result = await PollForCompletionAsync(transcriptionId, cancellationToken);
                
                if (result.Status != "completed")
                {
                    throw new InvalidOperationException($"Transcription failed with status: {result.Status}, Error: {result.Error}");
                }
                
                // Step 4: Map to domain model
                return MapToMediaTranscript(result, Path.GetFileName(filePath));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred during transcription process for file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Maps a TranscriptionResponseDto to a MediaTranscript domain model.
        /// </summary>
        /// <param name="response">The transcription response.</param>
        /// <param name="fileName">The name of the audio file.</param>
        /// <returns>A MediaTranscript domain model.</returns>
        private MediaTranscript MapToMediaTranscript(TranscriptionResponseDto response, string fileName)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var segments = new List<TranscriptSegment>();
            
            // If speaker diarization was enabled, use utterances
            if (response.SpeakerLabels && response.Utterances != null && response.Utterances.Count > 0)
            {
                foreach (var utterance in response.Utterances)
                {
                    var segment = new TranscriptSegment
                    {
                        Id = Guid.NewGuid().ToString(),
                        MediaId = response.Id,
                        Text = utterance.Text,
                        StartTime = utterance.Start,
                        EndTime = utterance.End,
                        Confidence = utterance.Confidence,
                        Speaker = utterance.Speaker,
                        Sentiment = GetSentimentForSegment(response.SentimentAnalysisResults, utterance.Start, utterance.End)
                    };
                    
                    // Add words if available
                    if (response.Words != null)
                    {
                        segment.Words = GetWordsForTimeRange(response.Words, utterance.Start, utterance.End);
                    }
                    
                    segments.Add(segment);
                }
            }
            // Otherwise, create segments based on punctuation or time gaps
            else if (response.Words != null && response.Words.Count > 0)
            {
                // This is a simplified approach - in a real implementation, you would
                // create segments based on punctuation or time gaps
                var currentSegment = new List<WordDto>();
                var currentStart = response.Words[0].Start;
                var currentSpeaker = response.Words[0].Speaker ?? "speaker_0";
                
                foreach (var word in response.Words)
                {
                    // If there's a significant time gap or speaker change, create a new segment
                    if (currentSegment.Count > 0 && 
                        (word.Start - currentSegment[^1].End > 1000 || // 1 second gap
                         word.Speaker != currentSpeaker && !string.IsNullOrEmpty(word.Speaker)))
                    {
                        var segmentText = string.Join(" ", currentSegment.Select(w => w.Text));
                        var segmentEnd = currentSegment[^1].End;
                        
                        segments.Add(new TranscriptSegment
                        {
                            Id = Guid.NewGuid().ToString(),
                            MediaId = response.Id,
                            Text = segmentText,
                            StartTime = currentStart,
                            EndTime = segmentEnd,
                            Confidence = currentSegment.Average(w => w.Confidence),
                            Speaker = currentSpeaker,
                            Sentiment = GetSentimentForSegment(response.SentimentAnalysisResults, currentStart, segmentEnd),
                            Words = currentSegment.Select(w => new WordInfo
                            {
                                Text = w.Text,
                                StartTime = w.Start,
                                EndTime = w.End,
                                Confidence = w.Confidence
                            }).ToList()
                        });
                        
                        // Start a new segment
                        currentSegment.Clear();
                        currentStart = word.Start;
                        currentSpeaker = word.Speaker ?? currentSpeaker;
                    }
                    
                    currentSegment.Add(word);
                }
                
                // Add the last segment
                if (currentSegment.Count > 0)
                {
                    var segmentText = string.Join(" ", currentSegment.Select(w => w.Text));
                    var segmentEnd = currentSegment[^1].End;
                    
                    segments.Add(new TranscriptSegment
                    {
                        Id = Guid.NewGuid().ToString(),
                        MediaId = response.Id,
                        Text = segmentText,
                        StartTime = currentStart,
                        EndTime = segmentEnd,
                        Confidence = currentSegment.Average(w => w.Confidence),
                        Speaker = currentSpeaker,
                        Sentiment = GetSentimentForSegment(response.SentimentAnalysisResults, currentStart, segmentEnd),
                        Words = currentSegment.Select(w => new WordInfo
                        {
                            Text = w.Text,
                            StartTime = w.Start,
                            EndTime = w.End,
                            Confidence = w.Confidence
                        }).ToList()
                    });
                }
            }
            
            return new MediaTranscript
            {
                Id = response.Id,
                MediaId = response.Id,
                FullText = response.Text ?? string.Empty,
                Segments = segments,
                Language = "en", // Default language
                CreatedDate = DateTime.UtcNow,
                FileName = fileName
            };
        }

        /// <summary>
        /// Gets the sentiment for a segment based on the sentiment analysis results.
        /// </summary>
        /// <param name="sentimentResults">The sentiment analysis results.</param>
        /// <param name="start">The start time of the segment.</param>
        /// <param name="end">The end time of the segment.</param>
        /// <returns>The sentiment as a string.</returns>
        private string GetSentimentForSegment(List<SentimentAnalysisResultDto>? sentimentResults, int start, int end)
        {
            if (sentimentResults == null || sentimentResults.Count == 0)
                return "NEUTRAL";
            
            // Find sentiment results that overlap with this segment
            var overlappingResults = sentimentResults
                .Where(r => r.End >= start && r.Start <= end)
                .ToList();
            
            if (overlappingResults.Count == 0)
                return "NEUTRAL";
            
            // Count the occurrences of each sentiment
            var sentimentCounts = overlappingResults
                .GroupBy(r => r.Sentiment)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Return the most common sentiment
            return sentimentCounts
                .OrderByDescending(kvp => kvp.Value)
                .First()
                .Key;
        }

        /// <summary>
        /// Gets the words for a specific time range.
        /// </summary>
        /// <param name="words">All words in the transcript.</param>
        /// <param name="start">The start time of the range.</param>
        /// <param name="end">The end time of the range.</param>
        /// <returns>A list of WordInfo objects for the specified time range.</returns>
        private List<WordInfo> GetWordsForTimeRange(List<WordDto> words, int start, int end)
        {
            return words
                .Where(w => w.End >= start && w.Start <= end)
                .Select(w => new WordInfo
                {
                    Text = w.Text,
                    StartTime = w.Start,
                    EndTime = w.End,
                    Confidence = w.Confidence
                })
                .ToList();
        }

        /// <summary>
        /// Maps a transcription status string to the TranscriptionStatus enum.
        /// </summary>
        /// <param name="status">The status string from the API.</param>
        /// <returns>The corresponding TranscriptionStatus enum value.</returns>
        public static TranscriptionStatus MapTranscriptionStatusStatic(string status)
        {
            return status switch
            {
                "queued" => TranscriptionStatus.Queued,
                "processing" => TranscriptionStatus.Processing,
                "completed" => TranscriptionStatus.Completed,
                "error" => TranscriptionStatus.Failed,
                _ => TranscriptionStatus.Unknown
            };
        }
    }
}
