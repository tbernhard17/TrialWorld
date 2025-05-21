using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace TrialWorld.Presentation.ViewModels
{
    /// <summary>
    /// ViewModel for silence detection settings
    /// </summary>
    public class SilenceDetectionViewModel : INotifyPropertyChanged
    {
        // Default to 10 seconds padding on each end for smoother transitions
        private int _padding = 10;
        
        // Default noise ceiling in dB - sounds below this level are considered silence
        private int _noiseCeiling = -30;
        
        // Progress tracking for silence detection
        private double _silenceDetectionProgress;
        
        // Status tracking
        private bool _isDetectingSilence;

        public int Padding
        {
            get => _padding;
            set { _padding = value; OnPropertyChanged(); }
        }

        public int NoiseCeiling
        {
            get => _noiseCeiling;
            set { _noiseCeiling = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Gets or sets the silence detection progress (0-100)
        /// </summary>
        public double SilenceDetectionProgress
        {
            get => _silenceDetectionProgress;
            set 
            { 
                _silenceDetectionProgress = value; 
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether silence detection is currently in progress
        /// </summary>
        public bool IsDetectingSilence
        {
            get => _isDetectingSilence;
            set 
            { 
                _isDetectingSilence = value; 
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Resets the silence detection progress to 0
        /// </summary>
        public void ResetProgress()
        {
            SilenceDetectionProgress = 0;
            IsDetectingSilence = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}