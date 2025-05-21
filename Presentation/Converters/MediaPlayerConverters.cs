using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    // Removed duplicate TimeSpanToStringConverter. Use the standalone TimeSpanToStringConverter.cs file instead.

    /// <summary>
    /// Converts a boolean value to a Visibility value for media player elements
    /// </summary>
    public class MediaPlayerBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = (parameter?.ToString()?.ToLower() ?? "") == "invert";
            bool visibility = value != null && (bool)value;

            if (invert)
                visibility = !visibility;

            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool invert = (parameter?.ToString()?.ToLower() ?? "") == "invert";
                bool result = visibility == Visibility.Visible;

                if (invert)
                    result = !result;

                return result;
            }

            return false;
        }
    }

    // Removed duplicate NullToVisibilityConverter. Use the standalone NullToVisibilityConverter.cs file instead.


    /// <summary>
    /// Inverses a boolean value
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return true;
        }
    }

    // Removed duplicate NullToVisibilityConverter. Use the standalone NullToVisibilityConverter.cs file instead.
}