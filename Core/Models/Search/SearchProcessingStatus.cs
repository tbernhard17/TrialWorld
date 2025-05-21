using System;
using System.Collections.Generic;

namespace TrialWorld.Core.Models.Search
{
    /// <summary>
    /// Represents the current processing status of the search database.
    /// </summary>
    public class SearchProcessingStatus
    {
        /// <summary>
        /// Indicates whether the search database is currently processing files.
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// The current operation being performed (e.g., "Indexing", "Optimizing", "Rebuilding").
        /// </summary>
        public string CurrentOperation { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the media file currently being processed, if applicable.
        /// </summary>
        public string? CurrentMediaId { get; set; }

        /// <summary>
        /// The name of the file currently being processed, if applicable.
        /// </summary>
        public string? CurrentFileName { get; set; }

        /// <summary>
        /// The timestamp when the current processing operation started.
        /// </summary>
        public DateTime OperationStartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The estimated completion percentage of the current operation (0-100).
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// The number of files processed in the current operation.
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// The total number of files to process in the current operation.
        /// </summary>
        public int TotalFilesToProcess { get; set; }

        /// <summary>
        /// The estimated time remaining for the current operation, if available.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// Any error messages encountered during processing.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// Additional status information specific to the current operation.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new instance of the SearchProcessingStatus class with IsProcessing set to false.
        /// </summary>
        public static SearchProcessingStatus Idle => new SearchProcessingStatus
        {
            IsProcessing = false,
            CurrentOperation = "Idle",
            ProgressPercentage = 100
        };

        /// <summary>
        /// Creates a new instance of the SearchProcessingStatus class for an indexing operation.
        /// </summary>
        public static SearchProcessingStatus StartIndexing(int totalFilesToProcess) => new SearchProcessingStatus
        {
            IsProcessing = true,
            CurrentOperation = "Indexing",
            OperationStartedAt = DateTime.UtcNow,
            ProgressPercentage = 0,
            FilesProcessed = 0,
            TotalFilesToProcess = totalFilesToProcess
        };

        /// <summary>
        /// Creates a new instance of the SearchProcessingStatus class for an optimization operation.
        /// </summary>
        public static SearchProcessingStatus StartOptimizing() => new SearchProcessingStatus
        {
            IsProcessing = true,
            CurrentOperation = "Optimizing",
            OperationStartedAt = DateTime.UtcNow,
            ProgressPercentage = 0
        };

        /// <summary>
        /// Updates the progress of the current operation.
        /// </summary>
        public void UpdateProgress(int filesProcessed, string? currentMediaId = null, string? currentFileName = null)
        {
            FilesProcessed = filesProcessed;
            CurrentMediaId = currentMediaId;
            CurrentFileName = currentFileName;
            
            if (TotalFilesToProcess > 0)
            {
                ProgressPercentage = Math.Min(100, (filesProcessed * 100.0) / TotalFilesToProcess);
                
                // Calculate estimated time remaining
                var elapsed = DateTime.UtcNow - OperationStartedAt;
                if (filesProcessed > 0 && ProgressPercentage < 100)
                {
                    var totalEstimatedTime = TimeSpan.FromTicks((long)(elapsed.Ticks / (ProgressPercentage / 100.0)));
                    EstimatedTimeRemaining = totalEstimatedTime - elapsed;
                }
                else
                {
                    EstimatedTimeRemaining = null;
                }
            }
        }

        /// <summary>
        /// Adds an error message to the status.
        /// </summary>
        public void AddError(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessages.Add(errorMessage);
            }
        }

        /// <summary>
        /// Completes the current operation.
        /// </summary>
        public void CompleteOperation()
        {
            IsProcessing = false;
            ProgressPercentage = 100;
            EstimatedTimeRemaining = null;
            CurrentOperation = "Completed";
        }
    }
}