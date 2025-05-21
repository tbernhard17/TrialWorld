using System;
using Microsoft.Extensions.Configuration;

namespace TrialWorld.Core.Config
{
    /// <summary>
    /// Feature flags to support code transition and migration
    /// </summary>
    public class FeatureFlags
    {
        /// <summary>
        /// Gets or sets whether to use the new media processing pipeline implementation
        /// </summary>
        public bool UseNewMediaProcessingPipeline { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the new emotion analysis implementation
        /// </summary>
        public bool UseNewEmotionAnalysis { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the new transcription service
        /// </summary>
        public bool UseNewTranscriptionService { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the new media indexer implementation
        /// </summary>
        public bool UseNewMediaIndexer { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the new media enhancement service
        /// </summary>
        public bool UseNewMediaEnhancement { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the new search engine implementation
        /// </summary>
        public bool UseNewSearchEngine { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use the consolidated directory structure
        /// </summary>
        public bool UseConsolidatedDirectoryStructure { get; set; } = false;

        /// <summary>
        /// Feature flag for enabling detailed FFmpeg command logging.
        /// </summary>
        public bool LogFFmpegCommands { get; set; } = false;
    }

    /// <summary>
    /// Extensions for working with feature flags
    /// </summary>
    public static class FeatureFlagExtensions
    {
        /// <summary>
        /// Configures feature flags from configuration
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <returns>Configured feature flags</returns>
        public static FeatureFlags GetFeatureFlags(this IConfiguration configuration)
        {
            var featureFlags = new FeatureFlags();
            configuration.GetSection("FeatureFlags").Bind(featureFlags);
            return featureFlags;
        }
    }
}