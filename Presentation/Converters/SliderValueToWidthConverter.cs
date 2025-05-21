using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    public class SliderValueToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sliderValue)
            {
                // Example: scale slider value to width (identity for placeholder)
                return sliderValue;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}