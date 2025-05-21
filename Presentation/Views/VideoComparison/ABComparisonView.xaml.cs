using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrialWorld.Presentation.Views.VideoComparison
{
    public partial class ABComparisonView : Window, INotifyPropertyChanged
    {
        private double _noiseReduction = 65;
        private double _bassBoost = 30;
        private double _trebleBoost = 40;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double NoiseReduction
        {
            get => _noiseReduction;
            set
            {
                if (_noiseReduction != value)
                {
                    _noiseReduction = value;
                    OnPropertyChanged();
                    // Updates() removed
                }
            }
        }

        public double BassBoost
        {
            get => _bassBoost;
            set
            {
                if (_bassBoost != value)
                {
                    _bassBoost = value;
                    OnPropertyChanged();
                    // Updates() removed
                }
            }
        }

        public double TrebleBoost
        {
            get => _trebleBoost;
            set
            {
                if (_trebleBoost != value)
                {
                    _trebleBoost = value;
                    OnPropertyChanged();
                    // Updates() removed
                }
            }
        }

        public ABComparisonView()
        {
            InitializeComponent();
            DataContext = this;

            // Set up media elements
            OriginalPlayer.MediaOpened += MediaElement_MediaOpened;
            // EnhancedPlayer removed

            // Sync playback positions
            // SyncPlayback removed
        }

        public void LoadVideo(string originalPath)
        {
            OriginalPlayer.Source = new Uri(originalPath);
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                // Ensure both videos start at the same position
                mediaElement.Position = TimeSpan.Zero;
            }
        }

        // SyncPlayback removed

        // Updates method removed

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            OriginalPlayer.Play();
            // EnhancedPlayer removed
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            OriginalPlayer.Pause();
            // EnhancedPlayer removed
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            OriginalPlayer.Stop();
            // EnhancedPlayer removed
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Cleanup
            // SyncPlayback removed
            OriginalPlayer.Close();
            // EnhancedPlayer removed
        }
    }
}