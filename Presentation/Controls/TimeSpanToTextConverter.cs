using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Converts a TimeSpan value to a formatted time string for display
    /// </summary>
    public class TimeSpanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle TimeSpan directly
            if (value is TimeSpan timeSpan)
            {
                return FormatTimeSpan(timeSpan);
            }

            // Handle double as seconds
            if (value is double seconds)
            {
                return FormatTimeSpan(TimeSpan.FromSeconds(seconds));
            }

            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter doesn't need to convert back
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Formats a TimeSpan as a readable time string
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format</param>
        /// <returns>Formatted string as "HH:MM:SS" or "MM:SS" if hours is 0</returns>
        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.Hours > 0
                ? $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }

    /// <summary>
    /// Converter that formats current and total duration as "currentTime / totalTime"
    /// </summary>
    public class MediaPositionTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return "00:00 / 00:00";

            var timeConverter = new TimeSpanToTextConverter();

            // Get current position
            string currentPosition = "00:00";
            if (values[0] != null && values[0] != DependencyProperty.UnsetValue)
            {
                currentPosition = (string)timeConverter.Convert(values[0], typeof(string), "", culture);
            }

            // Get total duration
            string totalDuration = "00:00";
            if (values[1] != null && values[1] != DependencyProperty.UnsetValue)
            {
                totalDuration = (string)timeConverter.Convert(values[1], typeof(string), "", culture);
            }

            return $"{currentPosition} / {totalDuration}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}