using System;
using System.Collections.Generic;
using System.Text.Json;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Models.Configuration
{
    /// <summary>
    /// Application settings model that implements IAppSettingsService
    /// </summary>
    public class AppSettings : IAppSettingsService
    {
        private readonly Dictionary<string, object> _settingsCache = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets a setting value of the specified type
        /// </summary>
        /// <typeparam name="T">Type of the setting value</typeparam>
        /// <param name="key">Setting key (can use colon notation for nested settings)</param>
        /// <param name="defaultValue">Optional default value if setting is not found</param>
        /// <returns>The setting value or default if not found</returns>
        public T? GetSetting<T>(string key, T? defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;
                
            if (_settingsCache.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
                    
                try
                {
                    // Try to convert the value to the requested type
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Sets a setting value
        /// </summary>
        /// <typeparam name="T">Type of the setting value</typeparam>
        /// <param name="key">Setting key, can be a hierarchical path like "Logging:MinimumLevel"</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if setting was successfully saved, false otherwise</returns>
        public bool SetSetting<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            _settingsCache[key] = value!;
            return true;
        }
        
        /// <summary>
        /// Saves all changes to settings
        /// </summary>
        /// <returns>True if settings were successfully saved, false otherwise</returns>
        public bool SaveSettings()
        {
            // In a real implementation, this would save to a file or database
            return true;
        }
        
        /// <summary>
        /// Gets or sets the FFmpeg settings
        /// </summary>
        public FFmpegSettings FFmpeg { get; set; } = new FFmpegSettings();
        
        /// <summary>
        /// Gets or sets the AssemblyAI settings
        /// </summary>
        public AssemblyAISettings AssemblyAI { get; set; } = new AssemblyAISettings();
        
        /// <summary>
        /// Gets or sets the media storage settings
        /// </summary>
        public MediaStorageSettings MediaStorage { get; set; } = new MediaStorageSettings();
        
        /// <summary>
        /// Gets or sets the search settings
        /// </summary>
        public SearchSettings Search { get; set; } = new SearchSettings();
        
        /// <summary>
        /// Gets or sets the export settings
        /// </summary>
        public ExportSettings Export { get; set; } = new ExportSettings();
        
        /// <summary>
        /// Gets or sets the UI settings
        /// </summary>
        public UISettings UI { get; set; } = new UISettings();
        
        /// <summary>
        /// Gets or sets the transcription paths
        /// </summary>
        public TranscriptionPathSettings TranscriptionPaths { get; set; } = new TranscriptionPathSettings();
        
        /// <summary>
        /// Gets or sets the feature flags
        /// </summary>
        public FeatureFlags FeatureFlags { get; set; } = new FeatureFlags();
    }
    
    /// <summary>
    /// FFmpeg settings
    /// </summary>
    public class FFmpegSettings
    {
        /// <summary>
        /// Gets or sets the FFmpeg executable path
        /// </summary>
        public string FFmpegPath { get; set; } = "./ffmpeg/ffmpeg.exe";
        
        /// <summary>
        /// Gets or sets the FFprobe executable path
        /// </summary>
        public string FFprobePath { get; set; } = "./ffmpeg/ffprobe.exe";
        
        /// <summary>
        /// Gets or sets the binary path
        /// </summary>
        public string BinaryPath { get; set; } = "C:\\FFmpeg\\bin";
        
        /// <summary>
        /// Gets or sets the maximum number of threads to use
        /// </summary>
        public int MaxThreads { get; set; } = 4;
        
        /// <summary>
        /// Gets or sets the temporary directory
        /// </summary>
        public string TempDirectory { get; set; } = "C:\\Temp\\FFmpeg";
        
        /// <summary>
        /// Gets or sets the queue polling interval in seconds
        /// </summary>
        public int QueuePollingIntervalSeconds { get; set; } = 10;
    }
    
    // AssemblyAISettings is defined in a separate file
    
    /// <summary>
    /// Media storage settings
    /// </summary>
    public class MediaStorageSettings
    {
        /// <summary>
        /// Gets or sets the base directory
        /// </summary>
        public string BaseDirectory { get; set; } = "C:\\TrialWorld\\Media";
        
        /// <summary>
        /// Gets or sets the raw directory
        /// </summary>
        public string RawDirectory { get; set; } = "Raw";
        
        /// <summary>
        /// Gets or sets the processed directory
        /// </summary>
        public string ProcessedDirectory { get; set; } = "Processed";
        
        /// <summary>
        /// Gets or sets the thumbnails directory
        /// </summary>
        public string ThumbnailsDirectory { get; set; } = "Thumbnails";
        
        /// <summary>
        /// Gets or sets the metadata directory
        /// </summary>
        public string MetadataDirectory { get; set; } = "Metadata";
        
        /// <summary>
        /// Gets or sets the cache directory
        /// </summary>
        public string CacheDirectory { get; set; } = "Cache";
        
        /// <summary>
        /// Gets or sets the transcripts directory
        /// </summary>
        public string TranscriptsDirectory { get; set; } = "Transcripts";
        
        /// <summary>
        /// Gets or sets the maximum cache size in GB
        /// </summary>
        public int MaxCacheSizeGB { get; set; } = 10;
    }
    
    /// <summary>
    /// Search settings
    /// </summary>
    public class SearchSettings
    {
        /// <summary>
        /// Gets or sets the index path
        /// </summary>
        public string IndexPath { get; set; } = "./SearchIndex";
        
        /// <summary>
        /// Gets or sets the maximum number of results
        /// </summary>
        public int MaxResults { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets the minimum score
        /// </summary>
        public double MinScore { get; set; } = 0.4;
        
        /// <summary>
        /// Gets or sets the default maximum results
        /// </summary>
        public int DefaultMaxResults { get; set; } = 50;
        
        /// <summary>
        /// Gets or sets whether to use in-memory index
        /// </summary>
        public bool UseInMemoryIndex { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the media extensions
        /// </summary>
        public string[] MediaExtensions { get; set; } = { ".mp4", ".mov", ".avi", ".mkv", ".mp3", ".wav", ".m4a" };
        
        /// <summary>
        /// Gets or sets whether to auto-update the index
        /// </summary>
        public bool AutoUpdateIndex { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the auto-update interval in minutes
        /// </summary>
        public int AutoUpdateIntervalMinutes { get; set; } = 60;
        
        /// <summary>
        /// Gets or sets whether to include private directories
        /// </summary>
        public bool IncludePrivateDirectories { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to extract topics
        /// </summary>
        public bool ExtractTopics { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enable transcription
        /// </summary>
        public bool EnableTranscription { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the enabled filters
        /// </summary>
        public string[] EnabledFilters { get; set; } = { "Keyword", "Speaker", "Date" };
    }
    
    /// <summary>
    /// Export settings
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// Gets or sets the default export path
        /// </summary>
        public string DefaultExportPath { get; set; } = "C:\\Users\\Public\\Documents\\TrialWorld\\Exports";
        
        /// <summary>
        /// Gets or sets the default video format
        /// </summary>
        public string DefaultVideoFormat { get; set; } = "mp4";
        
        /// <summary>
        /// Gets or sets the default audio format
        /// </summary>
        public string DefaultAudioFormat { get; set; } = "mp3";
    }
    
    /// <summary>
    /// UI settings
    /// </summary>
    public class UISettings
    {
        /// <summary>
        /// Gets or sets the theme
        /// </summary>
        public string Theme { get; set; } = "Light";
        
        /// <summary>
        /// Gets or sets the accent color
        /// </summary>
        public string AccentColor { get; set; } = "#1E90FF";
        
        /// <summary>
        /// Gets or sets the font size
        /// </summary>
        public string FontSize { get; set; } = "Medium";
    }
    
    // TranscriptionPathSettings is defined in a separate file
    
    /// <summary>
    /// Feature flags
    /// </summary>
    public class FeatureFlags
    {
        /// <summary>
        /// Gets or sets whether to use the new media processing pipeline
        /// </summary>
        public bool UseNewMediaProcessingPipeline { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to use the new transcription service
        /// </summary>
        public bool UseNewTranscriptionService { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to use the new media indexer
        /// </summary>
        public bool UseNewMediaIndexer { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to use the new media enhancement
        /// </summary>
        public bool UseNewMediaEnhancement { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to use the new search engine
        /// </summary>
        public bool UseNewSearchEngine { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to use the consolidated directory structure
        /// </summary>
        public bool UseConsolidatedDirectoryStructure { get; set; } = false;
    }
}
