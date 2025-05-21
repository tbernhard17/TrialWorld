using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrialWorld.Infrastructure.Transcription.Configuration;

namespace TrialWorld.AssemblyAIDiagnostic
{
    /// <summary>
    /// Manages secure storage and retrieval of API keys.
    /// </summary>
    public class ApiKeyManager : IDisposable
    {
        private readonly ILogger<ApiKeyManager> _logger;
        private readonly string _keyStorePath;
        private readonly byte[] _entropy;

        private const string ASSEMBLYAI_KEY_NAME = "AssemblyAI";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ApiKeyManager(ILogger<ApiKeyManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Create a directory for storing encrypted keys
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var companyPath = Path.Combine(appDataPath, "TrialWorld");
            _keyStorePath = Path.Combine(companyPath, "ApiKeys");
            
            if (!Directory.Exists(_keyStorePath))
            {
                Directory.CreateDirectory(_keyStorePath);
            }
            
            // Use machine-specific entropy for DPAPI
            _entropy = Encoding.UTF8.GetBytes(Environment.MachineName + "TrialWorld_AssemblyAI_Entropy");
        }

        /// <summary>
        /// Saves the AssemblyAI API key securely.
        /// </summary>
        /// <param name="apiKey">The API key to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveAssemblyAIKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            }
            
            try
            {
                var keyPath = Path.Combine(_keyStorePath, $"{ASSEMBLYAI_KEY_NAME}.bin");
                var keyBytes = Encoding.UTF8.GetBytes(apiKey);
                var encryptedBytes = ProtectedData.Protect(keyBytes, _entropy, DataProtectionScope.CurrentUser);
                
                await File.WriteAllBytesAsync(keyPath, encryptedBytes).ConfigureAwait(false);
                _logger.LogInformation("AssemblyAI API key saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save AssemblyAI API key");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the AssemblyAI API key.
        /// </summary>
        /// <returns>The API key if found; otherwise, null.</returns>
        public async Task<string?> GetAssemblyAIKeyAsync()
        {
            try
            {
                var keyPath = Path.Combine(_keyStorePath, $"{ASSEMBLYAI_KEY_NAME}.bin");
                
                if (!File.Exists(keyPath))
                {
                    _logger.LogWarning("AssemblyAI API key file not found");
                    return null;
                }
                
                var encryptedBytes = await File.ReadAllBytesAsync(keyPath).ConfigureAwait(false);
                var keyBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);
                
                return Encoding.UTF8.GetString(keyBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AssemblyAI API key");
                return null;
            }
        }

        /// <summary>
        /// Updates the configuration with the stored API key and returns the key.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <returns>The API key if found; otherwise, null.</returns>
        public async Task<string?> UpdateConfigurationWithApiKeyAsync(IConfiguration configuration)
        {
            try
            {
                var apiKey = await GetAssemblyAIKeyAsync().ConfigureAwait(false);
                
                if (!string.IsNullOrEmpty(apiKey))
                {
                    // Create a new configuration with the API key
                    var configDictionary = new Dictionary<string, string>
                    {
                        { "AssemblyAI:ApiKey", apiKey }
                    };
                    
                    var configBuilder = new ConfigurationBuilder();
                    configBuilder.AddInMemoryCollection(configDictionary);
                    var configToAdd = configBuilder.Build();
                    
                    // Add the new configuration to the existing one
                    ((IConfigurationRoot)configuration).Providers.ToList()
                        .ForEach(provider => ((ConfigurationRoot)configToAdd).Add(provider));
                    
                    _logger.LogInformation("Configuration updated with AssemblyAI API key");
                }
                else
                {
                    _logger.LogWarning("No AssemblyAI API key found to update configuration");
                }
                
                return apiKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update configuration with AssemblyAI API key");
                return null;
            }
        }
        
        /// <summary>
        /// Updates the AssemblyAI options with the stored API key.
        /// </summary>
        /// <param name="options">The options to update.</param>
        /// <returns>True if the options were updated successfully; otherwise, false.</returns>
        public async Task<bool> UpdateAssemblyAIOptionsAsync(IOptions<AssemblyAIOptions> options)
        {
            try
            {
                var apiKey = await GetAssemblyAIKeyAsync().ConfigureAwait(false);
                
                if (!string.IsNullOrEmpty(apiKey) && options != null && options.Value != null)
                {
                    // Use reflection to set the ApiKey property
                    var optionsType = options.Value.GetType();
                    var apiKeyProperty = optionsType.GetProperty("ApiKey");
                    
                    if (apiKeyProperty != null)
                    {
                        apiKeyProperty.SetValue(options.Value, apiKey);
                        _logger.LogInformation("AssemblyAI options updated with API key");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("ApiKey property not found in AssemblyAI options");
                    }
                }
                else
                {
                    _logger.LogWarning("No AssemblyAI API key found or options is null");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update AssemblyAI options with API key");
                return false;
            }
        }
        
        /// <summary>
        /// Disposes resources used by the ApiKeyManager.
        /// </summary>
        public void Dispose()
        {
            // Clean up any resources if needed
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Updates the configuration with the stored API key.
        /// </summary>
        /// <param name="configuration">The configuration to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateConfigurationWithStoredKeyAsync(IConfiguration configuration)
        {
            var apiKey = await GetAssemblyAIKeyAsync().ConfigureAwait(false);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Use reflection to set the API key in the configuration
                var configType = configuration.GetType();
                var dataProperty = configType.GetProperty("Data");
                
                if (dataProperty != null)
                {
                    var data = dataProperty.GetValue(configuration) as System.Collections.Generic.IDictionary<string, object>;
                    if (data != null && data.ContainsKey("AssemblyAI"))
                    {
                        var assemblyAIConfig = data["AssemblyAI"] as System.Collections.Generic.IDictionary<string, object>;
                        if (assemblyAIConfig != null)
                        {
                            assemblyAIConfig["ApiKey"] = apiKey;
                            _logger.LogInformation("Configuration updated with stored AssemblyAI API key");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether an AssemblyAI API key is stored.
        /// </summary>
        /// <returns>True if an API key is stored; otherwise, false.</returns>
        public bool HasStoredAssemblyAIKey()
        {
            var keyPath = Path.Combine(_keyStorePath, $"{ASSEMBLYAI_KEY_NAME}.bin");
            return File.Exists(keyPath);
        }
    }
}
