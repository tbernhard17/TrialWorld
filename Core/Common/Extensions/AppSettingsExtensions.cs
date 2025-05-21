using System;
using System.Collections.Generic;
using System.IO;
using TrialWorld.Core.Common.Interfaces;
using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Core.Common.Extensions
{
    /// <summary>
    /// Extension methods for IAppSettingsService to provide strongly-typed access to configuration sections
    /// </summary>
    public static class AppSettingsExtensions
    {
        /// <summary>
        /// Gets the Search settings section
        /// </summary>
        /// <param name="settings">The settings service</param>
        /// <returns>Search settings</returns>
        public static SearchSettings GetSearchSettings(this IAppSettingsService settings)
        {
            return settings.GetSetting<SearchSettings>("Search") ?? new SearchSettings
            {
                MediaExtensions = new[] { ".mp3", ".wav", ".mp4", ".m4a", ".mkv", ".mov", ".avi" }
            };
        }

        /// <summary>
        /// Gets the Export settings section
        /// </summary>
        /// <param name="settings">The settings service</param>
        /// <returns>Export settings</returns>
        public static ExportSettings GetExportSettings(this IAppSettingsService settings)
        {
            return settings.GetSetting<ExportSettings>("Export") ?? new ExportSettings
            {
                DefaultExportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrialWorld", "Transcripts")
            };
        }
    }
}
