using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    /// <summary>
    /// Inverts a boolean value
    /// </summary>
    public class BooleanInverterConverter : IValueConverter
    {
        /// <summary>
        /// Inverts a boolean value (true becomes false, false becomes true)
        /// </summary>
        /// <param name="value">The boolean value to invert</param>
        /// <param name="targetType">The type of the binding target property</param>
        /// <param name="parameter">The converter parameter to use</param>
        /// <param name="culture">The culture to use in the converter</param>
        /// <returns>The inverted boolean value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return false;
        }

        /// <summary>
        /// Inverts a boolean value (true becomes false, false becomes true)
        /// </summary>
        /// <param name="value">The boolean value to invert</param>
        /// <param name="targetType">The type of the binding target property</param>
        /// <param name="parameter">The converter parameter to use</param>
        /// <param name="culture">The culture to use in the converter</param>
        /// <returns>The inverted boolean value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return false;
        }
    }
}
