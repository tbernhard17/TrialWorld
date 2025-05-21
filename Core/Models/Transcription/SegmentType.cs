using System;

namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the type of a transcript segment.
    /// </summary>
    public enum SegmentType
    {
        /// <summary>
        /// Default segment type.
        /// </summary>
        Default = 0,
        
        /// <summary>
        /// Utterance segment (spoken by a speaker).
        /// </summary>
        Utterance = 1,
        
        /// <summary>
        /// Chapter segment (logical division of content).
        /// </summary>
        Chapter = 2,
        
        /// <summary>
        /// Summary segment (summarized content).
        /// </summary>
        Summary = 3,
        
        /// <summary>
        /// Highlight segment (important content).
        /// </summary>
        Highlight = 4
    }
}
