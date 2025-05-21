using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace TrialWorld.AssemblyAIDiagnostic.Views
{
    /// <summary>
    /// Interaction logic for ApiKeySetupDialog.xaml
    /// </summary>
    public partial class ApiKeySetupDialog : Window
    {
        private readonly ApiKeyManager _apiKeyManager;
        private readonly ILogger<ApiKeySetupDialog> _logger;

        /// <summary>
        /// Gets a value indicating whether the API key was saved successfully.
        /// </summary>
        public bool ApiKeySaved { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeySetupDialog"/> class.
        /// </summary>
        /// <param name="apiKeyManager">The API key manager.</param>
        /// <param name="logger">The logger.</param>
        public ApiKeySetupDialog(ApiKeyManager apiKeyManager, ILogger<ApiKeySetupDialog> logger)
        {
            _apiKeyManager = apiKeyManager ?? throw new ArgumentNullException(nameof(apiKeyManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeComponent();
            
            // Load existing API key if available
            LoadExistingApiKeyAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the existing API key asynchronously.
        /// </summary>
        private async Task LoadExistingApiKeyAsync()
        {
            try
            {
                var apiKey = await _apiKeyManager.GetAssemblyAIKeyAsync().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(apiKey))
                {
                    ApiKeyPasswordBox.Password = apiKey;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load existing API key");
            }
        }

        /// <summary>
        /// Handles the PasswordChanged event of the ApiKeyPasswordBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password);
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var apiKey = ApiKeyPasswordBox.Password;
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    ShowError("API key cannot be empty.");
                    return;
                }

                // Disable UI elements during save
                SaveButton.IsEnabled = false;
                ApiKeyPasswordBox.IsEnabled = false;
                ShowError(string.Empty);

                // Save the API key
                await _apiKeyManager.SaveAssemblyAIKeyAsync(apiKey).ConfigureAwait(true);
                
                ApiKeySaved = true;
                _logger.LogInformation("API key saved successfully");
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save API key");
                ShowError($"Failed to save API key: {ex.Message}");
                
                // Re-enable UI elements
                SaveButton.IsEnabled = true;
                ApiKeyPasswordBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the CancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        private void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
                ErrorMessageTextBlock.Text = string.Empty;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = message;
            }
        }
    }
}
