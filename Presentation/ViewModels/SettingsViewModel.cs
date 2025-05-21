using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using TrialWorld.Core.Models.Configuration;
using TrialWorld.Presentation.Commands;
using TrialWorld.Presentation.Interfaces; // Assuming IFileDialogService is here

namespace TrialWorld.Presentation.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly TranscriptionPathSettings _settings;
        private readonly IFileDialogService _fileDialogService; // Use interface for testability

        private string _extractedAudioPath;
        public string ExtractedAudioPath
        {
            get => _extractedAudioPath;
            set { _extractedAudioPath = value; OnPropertyChanged(); }
        }

        private string _transcriptionsPath;
        public string TranscriptionsPath
        {
            get => _transcriptionsPath;
            set { _transcriptionsPath = value; OnPropertyChanged(); }
        }

        private string _transcriptionDatabasePath;
        public string TranscriptionDatabasePath
        {
            get => _transcriptionDatabasePath;
            set { _transcriptionDatabasePath = value; OnPropertyChanged(); }
        }

        public ICommand BrowseExtractedAudioCommand { get; }
        public ICommand BrowseTranscriptionsCommand { get; }
        public ICommand BrowseDatabaseCommand { get; }
        public ICommand SaveSettingsCommand { get; } // TODO: Implement saving

        public SettingsViewModel(IOptions<TranscriptionPathSettings> settingsOptions, IFileDialogService fileDialogService)
        {
            _settings = settingsOptions.Value;
            _fileDialogService = fileDialogService;

            // Initialize properties from settings
            _extractedAudioPath = _settings.ExtractedAudioPath;
            _transcriptionsPath = _settings.TranscriptionsPath;
            _transcriptionDatabasePath = _settings.TranscriptionDatabasePath;

            BrowseExtractedAudioCommand = new RelayCommand(() => BrowseFolder(path => ExtractedAudioPath = path));
            BrowseTranscriptionsCommand = new RelayCommand(() => BrowseFolder(path => TranscriptionsPath = path));
            BrowseDatabaseCommand = new RelayCommand(() => BrowseFolder(path => TranscriptionDatabasePath = path));
            SaveSettingsCommand = new RelayCommand(SaveSettings, CanSaveSettings); // Placeholder
        }

        private async void BrowseFolder(Action<string> setPathAction)
        {
            string? selectedPath = await _fileDialogService.ShowFolderBrowserDialogAsync("Select Folder");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                setPathAction(selectedPath);
                // Trigger CanExecuteChanged for Save command if needed
                ((RelayCommand)SaveSettingsCommand).RaiseCanExecuteChanged();
            }
        }

        private bool CanSaveSettings()
        {
            // Check if paths have changed
            return ExtractedAudioPath != _settings.ExtractedAudioPath ||
                   TranscriptionsPath != _settings.TranscriptionsPath ||
                   TranscriptionDatabasePath != _settings.TranscriptionDatabasePath;
        }

        private void SaveSettings()
        {
            // TODO: Implement logic to save settings back to appsettings.json
            // This typically involves writing to the file or using a configuration management library.
            // For now, just update the in-memory _settings object (won't persist)
            _settings.ExtractedAudioPath = ExtractedAudioPath;
            _settings.TranscriptionsPath = TranscriptionsPath;
            _settings.TranscriptionDatabasePath = TranscriptionDatabasePath;

            // Disable save button after saving
             ((RelayCommand)SaveSettingsCommand).RaiseCanExecuteChanged();
             // Optionally show notification
             // _notificationService?.ShowSuccess("Settings Saved", "Transcription paths updated.");
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}