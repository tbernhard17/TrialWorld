using System;
using System.Globalization;
using System.Windows.Data;

namespace TrialWorld.Presentation.Converters
{
    /// <summary>
    /// Converts a string path to a Uri object for use with MediaElement.Source
    /// </summary>
    public class StringToUriConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    return new Uri(path);
                }
                catch (Exception)
                {
                    // If the path is not a valid URI, try to create a file URI
                    try
                    {
                        return new Uri(path, UriKind.Absolute);
                    }
                    catch (Exception)
                    {
                        // If that fails too, return null
                        return null;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Uri uri)
            {
                return uri.IsFile ? uri.LocalPath : uri.ToString();
            }
            return string.Empty;
        }
    }
}
