using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Converts a slider value to a width for the progress track
    /// </summary>
    public class SliderValueToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the slider value
            if (value is not double sliderValue)
                return 0.0;

            // Get the actual width of the slider from the parameter
            if (parameter is not FrameworkElement element)
                return 0.0;

            double actualWidth = element.ActualWidth;
            if (actualWidth <= 0)
                return 0.0;

            // Calculate the thumb width (default is 12 as set in our XAML)
            double thumbWidth = 12;

            // Adjust the width calculation to account for the thumb position
            double trackWidth = actualWidth - thumbWidth;

            // Calculate the proportion of the track that should be filled
            double proportion = sliderValue / 100.0; // Assuming slider is 0-100

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