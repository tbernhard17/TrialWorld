using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace TrialWorld.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Media Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.mp3;*.wav;*.aac|All Files|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                YouTubePlayer.Source = new System.Uri(ofd.FileName);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings dialog not implemented.");
        }

        private void TranscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Transcription features not wired in this demo.");
        }
    }
}
