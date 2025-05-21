using System;
using System.Collections.Generic;
using System.IO;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Infrastructure.Transcription.Services
{
    /// <summary>
    /// Provides unified progress tracking functionality for transcription operations.
    /// This eliminates duplicate progress calculation code across various implementations.
    /// </summary>
    public static class TranscriptionProgressTracker
    {
        // Phase weight allocation for single progress bar (total = 100%)
        private static readonly Dictionary<TranscriptionPhase, (double Start, double End)> PhaseRanges = 
            new Dictionary<TranscriptionPhase, (double Start, double End)>
            {
                { TranscriptionPhase.Queued,           (0.0, 2.0) },    // Initial 2%
                { TranscriptionPhase.SilenceDetection, (2.0, 15.0) },   // 13% for silence detection
                { TranscriptionPhase.Uploading,        (15.0, 40.0) },  // 25% for uploading
                { TranscriptionPhase.Submitted,        (40.0, 45.0) },  // 5% for submission
                { TranscriptionPhase.Processing,       (45.0, 98.0) },  // 53% for processing
                { TranscriptionPhase.Completed,        (98.0, 100.0) }  // Final 2% for completion
            };
            
        /// <summary>
        /// Calculates the overall progress percentage based on the current phase and phase-specific progress.
        /// </summary>
        /// <param name="phase">The current transcription phase</param>
        /// <param name="phaseProgress">The progress within the phase (0.0-1.0)</param>
        /// <returns>The overall progress percentage (0-100)</returns>
        public static double CalculateOverallProgress(TranscriptionPhase phase, double phaseProgress)
        {
            // Ensure phaseProgress is within valid range
            phaseProgress = Math.Clamp(phaseProgress, 0.0, 1.0);
            
            // Handle terminal states
            if (phase == TranscriptionPhase.Completed)
                return 100.0;
                
            if (phase == TranscriptionPhase.Failed)
                return 0.0;
                
            if (phase == TranscriptionPhase.Cancelled)
                return Math.Min(95.0, phaseProgress * 100.0); // Keep whatever progress we had, max 95%
            
            // If phase not found in dictionary, return direct percentage
            if (!PhaseRanges.TryGetValue(phase, out var range))
                return phaseProgress * 100.0;
                
            // Calculate weighted progress based on phase range
            var (start, end) = range;
            return start + (phaseProgress * (end - start));
        }
        
        /// <summary>
        /// Creates a new TranscriptionProgressUpdate with calculated overall progress.
        /// </summary>
        /// <param name="transcriptionId">Optional ID of the transcription</param>
        /// <param name="filePath">Path to the file being transcribed</param>
        /// <param name="phase">Current transcription phase</param>
        /// <param name="phaseProgress">Progress within the current phase (0.0-1.0)</param>
        /// <param name="status">Current transcription status</param>
        /// <param name="errorMessage">Optional error message</param>
        /// <returns>A new TranscriptionProgressUpdate instance</returns>
        public static TranscriptionProgressUpdate CreateProgressUpdate(
            string? transcriptionId,
            string filePath,
            TranscriptionPhase phase,
            double phaseProgress,
            TranscriptionStatus status,
            string? errorMessage = null)
        {
            // Create a progress update with proper mapping to the TranscriptionProgressUpdate properties
            return new TranscriptionProgressUpdate
            {
                TranscriptionId = transcriptionId ?? string.Empty,
                Phase = phase,
                PhaseProgress = phaseProgress * 100, // Convert 0-1 to percentage
                OverallProgress = CalculateOverallProgress(phase, phaseProgress),
                Message = $"Processing {Path.GetFileName(filePath)}",
                IsComplete = status == TranscriptionStatus.Completed,
                HasFailed = status == TranscriptionStatus.Failed,
                ErrorMessage = errorMessage
            };
        }
    }
}
