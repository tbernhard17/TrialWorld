using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Converts a TimeSpan position value to a width in pixels for the progress track
    /// </summary>
    public class TimeSpanToPixelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the current position as TimeSpan
            if (value is not TimeSpan currentPosition)
                return 0.0;

            // Parameter should be an array with [0] = control width element, [1] = total duration
            if (parameter is not object[] parameters || parameters.Length < 2)
                return 0.0;

            // Get the actual width of the slider
            if (parameters[0] is not FrameworkElement element)
                return 0.0;

            double actualWidth = element.ActualWidth;
            if (actualWidth <= 0)
                return 0.0;

            // Get the total duration
            TimeSpan totalDuration;
            if (parameters[1] is TimeSpan duration)
            {
                totalDuration = duration;
            }
            else if (parameters[1] is double durationSeconds)
            {
                totalDuration = TimeSpan.FromSeconds(durationSeconds);
            }
            else
            {
                return 0.0;
            }

            if (totalDuration.TotalSeconds <= 0)
                return 0.0;

            // Calculate the thumb width (default is 12 as set in our XAML)
            double thumbWidth = 12;

            // Calculate the actual track width (minus the thumb width)
            double trackWidth = actualWidth - thumbWidth;

            // Calculate the proportion of the track that should be filled
            double proportion = currentPosition.TotalSeconds / totalDuration.TotalSeconds;

            // Return the width in pixels
            return proportion * trackWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter doesn't need to convert back
            return DependencyProperty.UnsetValue;
        }
    }
}