using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Utilities
{
    /// <summary>
    /// Provides helper methods for JSON serialization and deserialization
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Default JSON serialization options
        /// </summary>
        public static JsonSerializerOptions DefaultOptions { get; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="options">Serialization options (uses DefaultOptions if not specified)</param>
        /// <returns>JSON string representation of the object</returns>
        public static string Serialize<T>(T obj, JsonSerializerOptions? options = null)
        {
            if (obj == null)
                return string.Empty;

            try
            {
                return JsonSerializer.Serialize(obj, options ?? DefaultOptions);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error serializing object of type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="options">Deserialization options (uses DefaultOptions if not specified)</param>
        /// <returns>Deserialized object</returns>
        public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing JSON to type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Tries to deserialize a JSON string to an object
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="result">Deserialized object (default if deserialization fails)</param>
        /// <param name="options">Deserialization options (uses DefaultOptions if not specified)</param>
        /// <returns>True if deserialization succeeded, false otherwise</returns>
        public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
        {
            result = default;

            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves an object to a JSON file
        /// </summary>
        /// <typeparam name="T">Type of object to save</typeparam>
        /// <param name="obj">Object to save</param>
        /// <param name="filePath">Path to the output file</param>
        /// <param name="options">Serialization options (uses DefaultOptions if not specified)</param>
        /// <param name="logger">Optional logger for error logging</param>
        /// <returns>True if the operation succeeded, false otherwise</returns>
        public static bool SaveToFile<T>(T obj, string filePath, JsonSerializerOptions? options = null, ILogger? logger = null)
        {
            if (obj == null)
            {
                logger?.LogError("Cannot save null object to JSON file");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot save JSON to empty file path");
                return false;
            }

            try
            {
                string json = Serialize(obj, options);
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error saving JSON to file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Loads an object from a JSON file
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="filePath">Path to the JSON file</param>
        /// <param name="options">Deserialization options (uses DefaultOptions if not specified)</param>
        /// <param name="logger">Optional logger for error logging</param>
        /// <returns>Deserialized object or default if the operation failed</returns>
        public static T? LoadFromFile<T>(string filePath, JsonSerializerOptions? options = null, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot load JSON from empty file path");
                return default;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogWarning("JSON file does not exist: {FilePath}", filePath);
                    return default;
                }

                string json = File.ReadAllText(filePath, Encoding.UTF8);
                return Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error loading JSON from file: {FilePath}", filePath);
                return default;
            }
        }

        /// <summary>
        /// Asynchronously saves an object to a JSON file
        /// </summary>
        /// <typeparam name="T">Type of object to save</typeparam>
        /// <param name="obj">Object to save</param>
        /// <param name="filePath">Path to the output file</param>
        /// <param name="options">Serialization options (uses DefaultOptions if not specified)</param>
        /// <param name="logger">Optional logger for error logging</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task<bool> SaveToFileAsync<T>(T obj, string filePath, JsonSerializerOptions? options = null, ILogger? logger = null)
        {
            if (obj == null)
            {
                logger?.LogError("Cannot save null object to JSON file");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot save JSON to empty file path");
                return false;
            }

            try
            {
                string json = Serialize(obj, options);
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error saving JSON to file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously loads an object from a JSON file
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="filePath">Path to the JSON file</param>
        /// <param name="options">Deserialization options (uses DefaultOptions if not specified)</param>
        /// <param name="logger">Optional logger for error logging</param>
        /// <returns>Task representing the asynchronous operation with the deserialized object</returns>
        public static async Task<T?> LoadFromFileAsync<T>(string filePath, JsonSerializerOptions? options = null, ILogger? logger = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Cannot load JSON from empty file path");
                return default;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogWarning("JSON file does not exist: {FilePath}", filePath);
                    return default;
                }

                string json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                return Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error loading JSON from file: {FilePath}", filePath);
                return default;
            }
        }

        /// <summary>
        /// Converts an object to a dictionary
        /// </summary>
        /// <typeparam name="T">Type of object to convert</typeparam>
        /// <param name="obj">Object to convert</param>
        /// <returns>Dictionary representation of the object</returns>
        public static Dictionary<string, object?>? ObjectToDictionary<T>(T obj)
        {
            if (obj == null)
                return null;

            try
            {
                string json = Serialize(obj);
                return Deserialize<Dictionary<string, object?>>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Merges two JSON strings
        /// </summary>
        /// <param name="json1">First JSON string</param>
        /// <param name="json2">Second JSON string</param>
        /// <param name="options">JSON options (uses DefaultOptions if not specified)</param>
        /// <returns>Merged JSON string</returns>
        public static string MergeJson(string json1, string json2, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrEmpty(json1))
                return json2 ?? string.Empty;
                
            if (string.IsNullOrEmpty(json2))
                return json1;

            try
            {
                options ??= DefaultOptions;
                
                var dict1 = Deserialize<Dictionary<string, JsonElement>>(json1, options);
                var dict2 = Deserialize<Dictionary<string, JsonElement>>(json2, options);
                
                if (dict1 == null || dict2 == null)
                    throw new JsonException("Failed to parse JSON objects as dictionaries");

                foreach (var kvp in dict2)
                {
                    dict1[kvp.Key] = kvp.Value;
                }
                
                return Serialize(dict1, options);
            }
            catch (Exception ex)
            {
                throw new JsonException("Error merging JSON objects", ex);
            }
        }
    }
}
