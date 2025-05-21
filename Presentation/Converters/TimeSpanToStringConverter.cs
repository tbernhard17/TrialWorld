using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                // Format as mm:ss or hh:mm:ss if needed
                return ts.Hours > 0 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && TimeSpan.TryParse(s, out var ts))
                return ts;
            return TimeSpan.Zero;
        }
    }
}
