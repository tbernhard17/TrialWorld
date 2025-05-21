namespace TrialWorld.Infrastructure.Export
{
    /// <summary>
    /// Configuration options for the export service
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Default quality level for exports (0-100)
        /// </summary>
        public int DefaultQuality { get; set; } = 80;

        /// <summary>
        /// Default folder name for exports within Documents folder
        /// </summary>
        public string DefaultExportFolderName { get; set; } = "TrialWorld Exports";

        /// <summary>
        /// Include metadata files by default
        /// </summary>
        public bool IncludeMetadataByDefault { get; set; } = true;

        /// <summary>
        /// Maximum parallel export tasks
        /// </summary>
        public int MaxParallelExports { get; set; } = 2;

        /// <summary>
        /// Default video codec
        /// </summary>
        public string DefaultVideoCodec { get; set; } = "libx264";

        /// <summary>
        /// Default audio codec
        /// </summary>
        public string DefaultAudioCodec { get; set; } = "aac";

        /// <summary>
        /// Default container format
        /// </summary>
        public string DefaultContainerFormat { get; set; } = "mp4";
    }
}