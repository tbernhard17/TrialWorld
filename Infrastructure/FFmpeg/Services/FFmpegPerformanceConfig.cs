using System;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Models;
using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Infrastructure.FFmpeg.Services
{
    /// <summary>
    /// Handles FFmpeg performance configuration and optimization
    /// </summary>
    public class FFmpegPerformanceConfig
    {
        private readonly ILogger _logger;
        
        public FFmpegPerformanceConfig(ILogger logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Gets optimized FFmpeg command line arguments based on selected performance config
        /// </summary>
        public string GetOptimizedArguments(TranscriptionPerformanceConfig config)
        {
            if (config == null)
                return string.Empty;
                
            string hwaccel = GetHwAccelArguments(config.HardwareAcceleration);
            string threads = config.Threads > 0 ? $" -threads {config.Threads}" : string.Empty;
            string preset = !string.IsNullOrEmpty(config.QualityPreset) ? $" -preset {config.QualityPreset}" : string.Empty;
            string memory = config.MaxMemoryMB > 0 ? $" -max_memory {config.MaxMemoryMB}M" : string.Empty;
            
            _logger.LogInformation("Using FFmpeg performance settings: Accel={Accel}, Threads={Threads}, Preset={Preset}, Memory={Memory}MB", 
                config.HardwareAcceleration, config.Threads, config.QualityPreset, config.MaxMemoryMB);
                
            return $"{hwaccel}{threads}{preset}{memory}";
        }
        
        /// <summary>
        /// Gets FFmpeg hardware acceleration arguments based on selected mode
        /// </summary>
        private string GetHwAccelArguments(TrialWorld.Core.Models.HardwareAccelerationMode mode)
        {
            // For best performance and compatibility based on GPU type
            return mode switch
            {
                HardwareAccelerationMode.CUDA => "-hwaccel cuda -hwaccel_output_format cuda",
                HardwareAccelerationMode.NVENC => "-hwaccel nvenc",
                HardwareAccelerationMode.QuickSync => "-hwaccel qsv -qsv_device auto",
                HardwareAccelerationMode.AMF => "-hwaccel amf",
                HardwareAccelerationMode.VAAPI => "-hwaccel vaapi -vaapi_device /dev/dri/renderD128",
                HardwareAccelerationMode.VideoToolbox => "-hwaccel videotoolbox",
                _ => string.Empty // No hardware acceleration
            };
        }
        
        /// <summary>
        /// Provides optimized transcoding arguments based on the file type and performance requirements
        /// </summary>
        public string GetOptimizedTranscodingArguments(string inputFile, TranscriptionPerformanceConfig config)
        {
            string ext = System.IO.Path.GetExtension(inputFile).ToLowerInvariant();
            string baseArgs = GetOptimizedArguments(config);
            
            // Optimization parameters tailored to the file type
            string additionalParams = ext switch {
                ".mp4" => "-movflags faststart",
                ".mkv" => "-c:v libx264 -crf 22",
                ".avi" => "-c:v libx264 -crf 23 -preset faster",
                ".mov" => "-c:v libx264 -crf 22",
                _ => ""
            };
            
            return $"{baseArgs} {additionalParams}".Trim();
        }
    }
}
