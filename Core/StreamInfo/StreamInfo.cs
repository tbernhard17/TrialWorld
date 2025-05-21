using System;
using System.IO;
using System.Collections.Generic;

namespace TrialWorld.Core.StreamInfo
{
    /// <summary>
    /// Represents information about a file stream resource.
    /// </summary>
    public class FileStreamInfo
    {
        /// <summary>
        /// Gets or sets the name of the stream.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path of the stream, if applicable.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the stream in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the stream.
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the underlying stream object.
        /// </summary>
        /// <remarks>
        /// This property should be properly disposed when no longer needed.
        /// </remarks>
        public Stream Stream { get; set; } = default!;

        /// <summary>
        /// Gets or sets additional metadata associated with the stream.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
