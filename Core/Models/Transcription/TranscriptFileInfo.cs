using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Available transcript file formats.
    /// </summary>
    public enum TranscriptFormat
    {
        /// <summary>
        /// Unknown format
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// JSON format (typically from AssemblyAI or other structured data)
        /// </summary>
        Json = 1,
        
        /// <summary>
        /// SRT subtitle format
        /// </summary>
        Srt = 2,
        
        /// <summary>
        /// Plain text format
        /// </summary>
        Text = 3
    }
    
    /// <summary>
    /// Information about an available transcript file.
    /// </summary>
    public class TranscriptFileInfo
    {
        /// <summary>
        /// Name of the transcript file.
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Full path to the transcript file.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Path relative to the transcript database root.
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long SizeBytes { get; set; }
        
        /// <summary>
        /// Last modified timestamp.
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Format of the transcript file.
        /// </summary>
        public TranscriptFormat Format { get; set; } = TranscriptFormat.Unknown;
        
        /// <summary>
        /// Gets or sets the file path (alias for FullPath for compatibility)
        /// </summary>
        public string FilePath
        {
            get => FullPath;
            set => FullPath = value;
        }
        
        /// <summary>
        /// Gets or sets the file size in bytes (alias for SizeBytes for compatibility)
        /// </summary>
        public long FileSize
        {
            get => SizeBytes;
            set => SizeBytes = value;
        }
        
        /// <summary>
        /// Gets or sets the created date (alias for LastModified for compatibility)
        /// </summary>
        public DateTime CreatedDate
        {
            get => LastModified;
            set => LastModified = value;
        }
        
        /// <summary>
        /// Gets or sets the transcript ID
        /// </summary>
        public string TranscriptId { get; set; } = string.Empty;
    }
}
