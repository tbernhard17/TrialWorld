using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility enum value with inverse logic.
    /// </summary>
    public class BooleanToVisibilityInverseConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility enum value with inverse logic.
        /// True -> Visibility.Collapsed, False -> Visibility.Visible
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional parameter. Not used.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Visibility.Collapsed if value is true; otherwise Visibility.Visible.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible;
        }

        /// <summary>
        /// Converts a Visibility enum value back to a boolean value with inverse logic.
        /// Visibility.Collapsed -> True, Otherwise -> False
        /// </summary>
        /// <param name="value">The Visibility value to convert back.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional parameter. Not used.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if value is Visibility.Collapsed; otherwise false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            
            return false;
        }
    }
}
