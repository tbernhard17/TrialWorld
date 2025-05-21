using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TrialWorld.Core.Models.Transcription;

namespace TrialWorld.Presentation.Converters
{
    /// <summary>
    /// Converts a TranscriptionStatus to a Brush for UI display
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a TranscriptionStatus to a Brush
        /// </summary>
        /// <param name="value">The TranscriptionStatus to convert</param>
        /// <param name="targetType">The type of the binding target property</param>
        /// <param name="parameter">The converter parameter to use</param>
        /// <param name="culture">The culture to use in the converter</param>
        /// <returns>A Brush representing the status</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TranscriptionStatus status)
            {
                return status switch
                {
                    TranscriptionStatus.Queued => new SolidColorBrush(Color.FromRgb(100, 100, 100)),     // Gray
                    TranscriptionStatus.Processing => new SolidColorBrush(Color.FromRgb(0, 120, 215)),   // Blue
                    TranscriptionStatus.Completed => new SolidColorBrush(Color.FromRgb(16, 124, 16)),    // Green
                    TranscriptionStatus.Failed => new SolidColorBrush(Color.FromRgb(232, 17, 35)),       // Red
                    TranscriptionStatus.Cancelled => new SolidColorBrush(Color.FromRgb(232, 140, 0)),    // Orange
                    _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))                              // Gray for unknown
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Not implemented - conversion is one way
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
