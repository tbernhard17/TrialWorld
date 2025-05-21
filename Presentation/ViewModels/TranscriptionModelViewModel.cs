using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models.Configuration;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel for managing transcription model selection in the UI
    /// </summary>
    public class TranscriptionModelViewModel : INotifyPropertyChanged
    {
        private readonly ITranscriptionService _transcriptionService;
        private AssemblyAIModel _selectedModel;

        /// <summary>
        /// The currently selected transcription model
        /// </summary>
        public AssemblyAIModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OnPropertyChanged();
                    UpdateTranscriptionService();
                }
            }
        }

        /// <summary>
        /// Display-friendly name of the current model
        /// </summary>
        public string SelectedModelDisplay => $"{SelectedModel} ({GetModelDescription(SelectedModel)})";

        /// <summary>
        /// Initializes a new instance of TranscriptionModelViewModel
        /// </summary>
        /// <param name="transcriptionService">The transcription service to update when model changes</param>
        /// <param name="initialModel">Initial model selection (defaults to Universal)</param>
        public TranscriptionModelViewModel(ITranscriptionService transcriptionService, AssemblyAIModel initialModel = AssemblyAIModel.Universal)
        {
            _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            _selectedModel = initialModel;
        }

        private void UpdateTranscriptionService()
        {
            // If the transcription service has a way to change model, call it here
            if (_transcriptionService is IConfigurableTranscriptionService configurableService)
            {
                configurableService.SetModel(SelectedModel);
            }
        }

        /// <summary>
        /// Gets a user-friendly description of the model
        /// </summary>
        public static string GetModelDescription(AssemblyAIModel model)
        {
            return model switch
            {
                AssemblyAIModel.Nova => "Lowest Cost",
                AssemblyAIModel.Universal => "Best for Most Languages",
                AssemblyAIModel.Slam1 => "Most Customizable (English only)",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the relative cost indicator of the model
        /// </summary>
        public static string GetModelCostIndicator(AssemblyAIModel model)
        {
            return model switch
            {
                AssemblyAIModel.Nova => "$",
                AssemblyAIModel.Universal => "$$",
                AssemblyAIModel.Slam1 => "$$$",
                _ => string.Empty
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
