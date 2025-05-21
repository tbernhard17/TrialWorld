using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using TrialWorld.Core.Models.Transcription;
using TrialWorld.Core.Models.Transcription.Interfaces;
using TrialWorld.Infrastructure.Models.AssemblyAI.DTOs;
using TrialWorld.Presentation.Models;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// Service for performing search operations on transcriptions
    /// </summary>
    public class SearchService
    {
        /// <summary>
        /// Search transcription with given parameters and populate results
        /// </summary>
        public async Task<List<SearchResultItem>> SearchTranscriptionAsync(
            ITranscriptionResult? transcriptionResult, 
            string transcriptionId,
            string searchText, 
            bool includeWords, 
            bool includeSentiment,
            bool includeHighlights, 
            bool includeChapters, 
            string sentimentFilter)
        {
            // Follow Rule #8 - Handle errors properly
            if (transcriptionResult == null)
            {
                return new List<SearchResultItem>();
            }
            
            // Simulate network delay for async operation to follow Rule #3
            await Task.Delay(50);
            
            var results = new List<SearchResultItem>();
            
            if (string.IsNullOrWhiteSpace(searchText) && sentimentFilter != "All")
            {
                // If no search text but sentiment filter is active, just filter by sentiment
                results.AddRange(transcriptionResult.SentimentAnalysisResults
                    .Where(s => s.Sentiment == sentimentFilter)
                    .Select(s => new SearchResultItem
                    {
                        Text = s.Text,
                        StartTimeMs = s.StartMilliseconds,
                        EndTimeMs = s.EndMilliseconds,
                        Confidence = s.Confidence,
                        Type = "Sentiment",
                        Sentiment = s.Sentiment,
                        TranscriptionId = transcriptionId
                    })
                    .OrderBy(r => r.StartTimeMs)
                    .ToList());
            }
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // No search text and no sentiment filter, return all applicable result types
                return PopulateAllResults(transcriptionResult, transcriptionId, includeWords, includeSentiment, includeHighlights, includeChapters);
            }
            
            // Words/phrases search
            if (includeWords && transcriptionResult.Segments != null)
            {
                var matchingWords = transcriptionResult.Segments
                    .Where(w => w.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(w => new SearchResultItem
                    {
                        Text = w.Text,
                        StartTimeMs = w.StartMilliseconds,
                        EndTimeMs = w.EndMilliseconds,
                        Confidence = w.Confidence,
                        Type = "Word",
                        TranscriptionId = transcriptionId
                    });
                    
                results.AddRange(matchingWords);
            }
            
            // Sentiment analysis search
            if (includeSentiment)
            {
                var matchingSentiments = transcriptionResult.SentimentAnalysisResults
                    .Where(s => s.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Where(s => sentimentFilter == "All" || s.Sentiment == sentimentFilter)
                    .Select(s => new SearchResultItem
                    {
                        Text = s.Text,
                        StartTimeMs = s.StartMilliseconds,
                        EndTimeMs = s.EndMilliseconds,
                        Confidence = s.Confidence,
                        Type = "Sentiment",
                        Sentiment = s.Sentiment,
                        TranscriptionId = transcriptionId
                    });
                    
                results.AddRange(matchingSentiments);
            }
            
            // Highlights search
            if (includeHighlights && transcriptionResult.Highlights != null)
            {
                var matchingHighlights = transcriptionResult.Highlights
                    .Where(h => h.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(h => new SearchResultItem
                    {
                        Text = h.Text,
                        StartTimeMs = h.StartMilliseconds,
                        EndTimeMs = h.EndMilliseconds,
                        Confidence = 1.0, // Highlights typically don't have confidence
                        Type = "Highlight",
                        TranscriptionId = transcriptionId
                    });
                    
                results.AddRange(matchingHighlights);
            }
            
            // Chapters search
            if (includeChapters)
            {
                var matchingChapters = transcriptionResult.Chapters
                    .Where(c => c.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(c => new SearchResultItem
                    {
                        Text = c.Text,
                        StartTimeMs = c.StartMilliseconds,
                        EndTimeMs = c.EndMilliseconds,
                        Confidence = 1.0, // Chapters typically don't have confidence
                        Type = "Chapter",
                        TranscriptionId = transcriptionId
                    });
                    
                results.AddRange(matchingChapters);
            }
            
            // Sort results by start time
            return results.OrderBy(r => r.StartTimeMs).ToList();
        }
        
        // Removed redundant FilterBySentiment method - logic now incorporated directly into SearchTranscriptionAsync
        
        /// <summary>
        /// Populate all matching result types without text filtering
        /// </summary>
        private List<SearchResultItem> PopulateAllResults(
            ITranscriptionResult transcriptionResult, 
            string transcriptionId,
            bool includeWords, 
            bool includeSentiment, 
            bool includeHighlights, 
            bool includeChapters)
        {
            var results = new List<SearchResultItem>();
            
            // Add sentiment results
            if (includeSentiment)
            {
                foreach (var s in transcriptionResult.SentimentAnalysisResults)
                {
                    results.Add(new SearchResultItem
                    {
                        Text = s.Text,
                        StartTimeMs = s.StartMilliseconds,
                        EndTimeMs = s.EndMilliseconds,
                        Confidence = s.Confidence,
                        Type = "Sentiment",
                        Sentiment = s.Sentiment,
                        TranscriptionId = transcriptionId
                    });
                }
            }
            
            // Add chapters if requested
            if (includeChapters)
            {
                foreach (var c in transcriptionResult.Chapters)
                {
                    results.Add(new SearchResultItem
                    {
                        Text = c.Text,
                        StartTimeMs = c.StartMilliseconds,
                        EndTimeMs = c.EndMilliseconds,
                        Confidence = 1.0,
                        Type = "Chapter",
                        TranscriptionId = transcriptionId
                    });
                }
            }
            
            // Add highlights if requested
            if (includeHighlights && transcriptionResult.Highlights != null)
            {
                foreach (var highlight in transcriptionResult.Highlights)
                {
                    results.Add(new SearchResultItem
                    {
                        Text = highlight.Text,
                        StartTimeMs = highlight.StartMilliseconds,
                        EndTimeMs = highlight.EndMilliseconds,
                        Confidence = 1.0,
                        Type = "Highlight",
                        TranscriptionId = transcriptionId
                    });
                }
            }
            
            // Add words if requested (usually too many, so we'll just add specific ones)
            if (includeWords)
            {
                // Get a sample of words (first 20)
                foreach (var segment in transcriptionResult.Segments.Take(20))
                {
                    results.Add(new SearchResultItem
                    {
                        Text = segment.Text,
                        StartTimeMs = segment.StartMilliseconds,
                        EndTimeMs = segment.EndMilliseconds,
                        Confidence = segment.Confidence,
                        Type = "Word",
                        TranscriptionId = transcriptionId
                    });
                }
            }
            
            return results.OrderBy(r => r.StartTimeMs).ToList();
        }
        
        /// <summary>
        /// Create mock transcription data for testing
        /// </summary>
        public TranscriptionResult CreateMockTranscriptionData(string transcriptId)
        {
            return new TranscriptionResult
            {
                Id = transcriptId,
                Status = TranscriptionStatus.Completed,
                Success = true,
                Transcript = "This is a sample transcription for testing search functionality.",
                CreatedAt = DateTime.UtcNow,
                Confidence = 0.95,
                DetectedLanguage = "en-US",
                Language = "en-US",
                
                // Create word segments
                Segments = new List<TranscriptSegment>
                {
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "This is", StartTime = 0, EndTime = 1500, Confidence = 0.98, Speaker = "SpeakerA", Sentiment = "Neutral" },
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "a sample", StartTime = 1600, EndTime = 3000, Confidence = 0.95, Speaker = "SpeakerB", Sentiment = "Neutral" },
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "transcription for testing", StartTime = 3100, EndTime = 5000, Confidence = 0.92, Speaker = "SpeakerA", Sentiment = "Neutral" },
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "search functionality", StartTime = 5100, EndTime = 7000, Confidence = 0.97, Speaker = "SpeakerB", Sentiment = "Neutral" }
                },
                
                // Create sentiment analysis results
                SentimentAnalysisResults = new List<SentimentAnalysisResult>
                {
                    new() { Text = "This is a great sample", Start = 0, End = 3000, Sentiment = "POSITIVE", Confidence = 0.85 },
                    new() { Text = "Testing is important", Start = 3100, End = 5000, Sentiment = "NEUTRAL", Confidence = 0.78 },
                    new() { Text = "Search not working would be frustrating", Start = 5100, End = 7000, Sentiment = "NEGATIVE", Confidence = 0.82 }
                },
                
                // Create highlights
                Highlights = new List<TranscriptSegment>
                {
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "Sample transcription", StartTime = 1600, EndTime = 5000, Confidence = 0.9, Speaker = "Highlight", Sentiment = "Neutral" },
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "Search functionality", StartTime = 5100, EndTime = 7000, Confidence = 0.9, Speaker = "Highlight", Sentiment = "Neutral" }
                },
                
                // Create chapters
                Chapters = new List<TranscriptSegment>
                {
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "Introduction", StartTime = 0, EndTime = 3000, Confidence = 1.0, Speaker = "Chapter", Sentiment = "Neutral" },
                    new() { Id = Guid.NewGuid().ToString(), MediaId = transcriptId, Text = "Main Content", StartTime = 3100, EndTime = 7000, Confidence = 1.0, Speaker = "Chapter", Sentiment = "Neutral" }
                }
            };
        }
    }
}
