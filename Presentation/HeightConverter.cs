using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation
{
    /// <summary>
    /// Converter that adjusts height by a specific factor 
    /// </summary>
    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                // Make the height 50% taller (multiply by 0.75 instead of 0.5)
                return d * 0.75;
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
