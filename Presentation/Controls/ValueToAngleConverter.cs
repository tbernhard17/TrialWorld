using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Converts a numeric value to an angle for use in circular progress indicators
    /// </summary>
    public class ValueToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = 0;
            double minimum = 0;
            double maximum = 100;

            // Get the value
            if (value is double val)
            {
                doubleValue = val;
            }
            else if (value is int intVal)
            {
                doubleValue = intVal;
            }
            else if (value is float floatVal)
            {
                doubleValue = floatVal;
            }
            else if (value is TimeSpan timeSpan)
            {
                doubleValue = timeSpan.TotalSeconds;
            }

            // If parameter contains min and max, extract them
            if (parameter is object[] parameters && parameters.Length >= 2)
            {
                if (parameters[0] is double min)
                    minimum = min;
                else if (parameters[0] is int intMin)
                    minimum = intMin;

                if (parameters[1] is double max)
                    maximum = max;
                else if (parameters[1] is int intMax)
                    maximum = intMax;
                else if (parameters[1] is TimeSpan maxTimeSpan)
                    maximum = maxTimeSpan.TotalSeconds;
            }

            // Calculate angle (0-360 degrees)
            // Clamp value between minimum and maximum
            doubleValue = Math.Max(minimum, Math.Min(maximum, doubleValue));

            // Calculate percentage
            double percentage = (doubleValue - minimum) / (maximum - minimum);

            // Convert percentage to angle (0-360 degrees)
            return percentage * 360;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter doesn't need to convert back
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}