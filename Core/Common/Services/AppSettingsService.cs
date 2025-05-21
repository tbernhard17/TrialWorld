using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    /// <summary>
    /// Implementation of application settings service using JSON storage
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly string _settingsFilePath;
        private readonly Dictionary<string, object> _settings;
        private readonly ILoggingService? _logger;
        private bool _isDirty = false;
        
        public AppSettingsService(ILoggingService? logger = null)
        {
            _logger = logger;
            
            // Determine settings file path
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "TrialWorld");
            
            // Ensure directory exists
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
            
            // Load or create settings
            _settings = LoadSettings();
            
            // Initialize default settings if needed
            InitializeDefaultSettings();
        }
        
        public T? GetSetting<T>(string key, T? defaultValue = default)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return defaultValue;
                }
                
                // Handle hierarchical keys like "Logging:MinimumLevel"
                var keys = key.Split(':');
                var currentDict = _settings;
                
                // Navigate through hierarchy
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    var currentKey = keys[i];
                    
                    if (!currentDict.ContainsKey(currentKey) || 
                        !(currentDict[currentKey] is Dictionary<string, object> nextDict))
                    {
                        return defaultValue;
                    }
                    
                    currentDict = nextDict;
                }
                
                // Get final key
                var finalKey = keys[keys.Length - 1];
                
                if (!currentDict.ContainsKey(finalKey))
                {
                    return defaultValue;
                }
                
                var value = currentDict[finalKey];
                
                // Convert to requested type
                if (value is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement, defaultValue);
                }
                else if (value is T typedValue)
                {
                    return typedValue;
                }
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error retrieving setting: {key}");
                return defaultValue;
            }
        }
        
        public bool SetSetting<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return false;
                }
                
                // Handle hierarchical keys like "Logging:MinimumLevel"
                var keys = key.Split(':');
                var currentDict = _settings;
                
                // Navigate through hierarchy and create missing dictionaries
                for (int i = 0; i < keys.Length - 1; i++)
                {
                    var currentKey = keys[i];
                    
                    if (!currentDict.ContainsKey(currentKey))
                    {
                        currentDict[currentKey] = new Dictionary<string, object>();
                    }
                    else if (!(currentDict[currentKey] is Dictionary<string, object>))
                    {
                        // Replace non-dictionary with dictionary
                        currentDict[currentKey] = new Dictionary<string, object>();
                    }
                    
                    currentDict = (Dictionary<string, object>)currentDict[currentKey];
                }
                
                // Set final key - Use null-forgiving operator if null is acceptable
                var finalKey = keys[keys.Length - 1];
                currentDict[finalKey] = (object?)value!; // Suppress CS8601
                
                _isDirty = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error setting value for setting: {key}");
                return false;
            }
        }
        
        public bool SaveSettings()
        {
            try
            {
                if (!_isDirty)
                {
                    return true;
                }
                
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(_settingsFilePath, json);
                
                _isDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving settings");
                return false;
            }
        }
        
        private Dictionary<string, object> LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };
                    
                    using var document = JsonDocument.Parse(json, new JsonDocumentOptions 
                    { 
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip
                    });
                    
                    return ConvertJsonElement(document.RootElement);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading settings, using defaults");
            }
            
            return new Dictionary<string, object>();
        }
        
        private void InitializeDefaultSettings()
        {
            // Initialize default settings if they don't exist
            var defaults = new Dictionary<string, object>
            {
                ["Logging"] = new Dictionary<string, object>
                {
                    ["MinimumLevel"] = "Information"
                },
                ["FFmpeg"] = new Dictionary<string, object>
                {
                    ["CustomPath"] = "",
                    ["DefaultQuality"] = 7
                },
                ["UI"] = new Dictionary<string, object>
                {
                    ["Theme"] = "System",
                    ["Language"] = "en-US"
                }
            };
            
            // Apply defaults for any missing settings
            foreach (var kvp in defaults)
            {
                if (!_settings.ContainsKey(kvp.Key))
                {
                    _settings[kvp.Key] = kvp.Value;
                    _isDirty = true;
                }
                else if (kvp.Value is Dictionary<string, object> subDefaults)
                {
                    if (!(_settings[kvp.Key] is Dictionary<string, object> subSettings))
                    {
                        _settings[kvp.Key] = kvp.Value;
                        _isDirty = true;
                        continue;
                    }
                    
                    foreach (var subKvp in subDefaults)
                    {
                        if (!subSettings.ContainsKey(subKvp.Key))
                        {
                            subSettings[subKvp.Key] = subKvp.Value;
                            _isDirty = true;
                        }
                    }
                }
            }
            
            if (_isDirty)
            {
                SaveSettings();
            }
        }
        
        private T? ConvertJsonElement<T>(JsonElement element, T? defaultValue)
        {
            try
            {
                var targetType = typeof(T);
                
                if (targetType == typeof(string))
                {
                    // GetString() can return null. Handle potential null for non-nullable T.
                    string? stringValue = element.GetString();
                    // If T is string, return stringValue (which could be null).
                    // If T is another non-nullable reference type, this cast might fail at runtime if stringValue is null.
                    // If T is a value type, this cast will fail.
                    // A more robust solution might involve TypeConverters or specific handling.
                    // For now, return default(T) if stringValue is null and T is expected to be non-null string.
                    // This cast is complex. Let's simplify: return the potentially null string directly if T is string.
                    if (typeof(T) == typeof(string))
                    {
                         return (T?)(object?)stringValue; // Cast to object? first
                    }
                    // Handle other types or throw if conversion isn't supported/safe
                    // For now, assume T is string or nullable string if we reach here based on the outer if.
                    return (T?)(object?)stringValue; // Allow potential null return
                }
                else if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    return (T?)(object)element.GetInt32();
                }
                else if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    return (T?)(object)element.GetDouble();
                }
                else if (targetType == typeof(bool) || targetType == typeof(bool?))
                {
                    return (T?)(object)element.GetBoolean();
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(element.GetRawText());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error converting JSON element");
                return defaultValue;
            }
        }
        
        private Dictionary<string, object> ConvertJsonElement(JsonElement element)
        {
            var result = new Dictionary<string, object>();
            
            if (element.ValueKind != JsonValueKind.Object)
            {
                return result;
            }
            
            foreach (var property in element.EnumerateObject())
            {
                // Handle Null first
                if (property.Value.ValueKind == JsonValueKind.Null)
                {
                    result[property.Name] = (object?)null!; // Explicitly using null-forgiving operator as we're handling null intentionally
                    continue; // Skip other checks
                }

                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        result[property.Name] = ConvertJsonElement(property.Value);
                        break;
                    case JsonValueKind.Array:
                        // Arrays might need deeper conversion depending on usage,
                        // but for now, store the JsonElement array.
                        result[property.Name] = property.Value.Clone(); // Clone to avoid issues with document disposal
                        break;
                    case JsonValueKind.String:
                        // Assign string? to object, which is fine.
                        result[property.Name] = property.Value.GetString() ?? string.Empty;
                        break;
                    case JsonValueKind.Number:
                        if (property.Value.TryGetInt32(out int intValue))
                        {
                            result[property.Name] = intValue;
                        }
                        else if (property.Value.TryGetInt64(out long longValue)) // Handle long integers
                        {
                             result[property.Name] = longValue;
                        }
                        else // Default to double
                        {
                            result[property.Name] = property.Value.GetDouble();
                        }
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        result[property.Name] = property.Value.GetBoolean();
                        break;
                    // Null case handled above
                    // default: // Optional: handle Undefined or log unexpected kinds
                    //    _logger?.LogWarning($"Unexpected JsonValueKind '{property.Value.ValueKind}' for property '{property.Name}'.");
                    //    break;
                }
            }
            
            return result;
        }
    }
}
