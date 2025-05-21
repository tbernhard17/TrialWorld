using System;

namespace TrialWorld.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for accessing application settings
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Gets a setting value of the specified type
        /// </summary>
        /// <typeparam name="T">Type of the setting value</typeparam>
        /// <param name="key">Setting key (can use colon notation for nested settings)</param>
        /// <param name="defaultValue">Optional default value if setting is not found</param>
        /// <returns>The setting value or default if not found</returns>
        T? GetSetting<T>(string key, T? defaultValue = default);
        
        /// <summary>
        /// Sets a setting value
        /// </summary>
        /// <typeparam name="T">Type of the setting value</typeparam>
        /// <param name="key">Setting key, can be a hierarchical path like "Logging:MinimumLevel"</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if setting was successfully saved, false otherwise</returns>
        bool SetSetting<T>(string key, T value);
        
        /// <summary>
        /// Saves all changes to settings
        /// </summary>
        /// <returns>True if settings were successfully saved, false otherwise</returns>
        bool SaveSettings();
    }
}