using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Converts a numeric seconds value to a TimeSpan
    /// </summary>
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double seconds)
            {
                return TimeSpan.FromSeconds(seconds);
            }

            if (value is int intSeconds)
            {
                return TimeSpan.FromSeconds(intSeconds);
            }

            if (value is float floatSeconds)
            {
                return TimeSpan.FromSeconds(floatSeconds);
            }

            if (value is string stringSeconds && double.TryParse(stringSeconds, out double parsedSeconds))
            {
                return TimeSpan.FromSeconds(parsedSeconds);
            }

            return TimeSpan.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                // Return the appropriate type based on targetType
                if (targetType == typeof(double))
                    return timeSpan.TotalSeconds;
                if (targetType == typeof(int))
                    return (int)timeSpan.TotalSeconds;
                if (targetType == typeof(float))
                    return (float)timeSpan.TotalSeconds;
                if (targetType == typeof(string))
                    return timeSpan.TotalSeconds.ToString(culture);

                // Default to double
                return timeSpan.TotalSeconds;
            }

            // Default values for each possible target type
            if (targetType == typeof(double))
                return 0.0;
            if (targetType == typeof(int))
                return 0;
            if (targetType == typeof(float))
                return 0.0f;
            if (targetType == typeof(string))
                return "0";

            return DependencyProperty.UnsetValue;
        }
    }
}