using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Utilities
{
    /// <summary>
    /// Extension methods for string manipulation and validation
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Truncates a string to a maximum length and adds an ellipsis if truncated
        /// </summary>
        /// <param name="str">String to truncate</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="ellipsis">Ellipsis string to append (default: "...")</param>
        /// <returns>Truncated string with ellipsis if needed</returns>
        public static string Truncate(this string str, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(str) || maxLength <= 0)
                return string.Empty;

            if (str.Length <= maxLength)
                return str;

            int endPosition = Math.Max(0, maxLength - ellipsis.Length);
            return str.Substring(0, endPosition) + ellipsis;
        }

        /// <summary>
        /// Converts a string to title case
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Title-cased string</returns>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Converts a camelCase or PascalCase string to a space-separated string
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Space-separated string</returns>
        public static string ToSpacedWords(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            // Insert a space before each uppercase letter that has a lowercase letter before it
            return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
        }

        /// <summary>
        /// Converts a string to a slug format (lowercase, dashes instead of spaces)
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>Slug-formatted string</returns>
        public static string ToSlug(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            // Remove special characters
            str = Regex.Replace(str, @"[^a-zA-Z0-9\s-]", "");
            
            // Convert to lowercase and replace spaces with dashes
            str = str.ToLower().Replace(" ", "-");
            
            // Remove multiple dashes
            str = Regex.Replace(str, @"-+", "-");
            
            // Trim dashes from the beginning and end
            return str.Trim('-');
        }

        /// <summary>
        /// Checks if a string is a valid email address format
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if the string is a valid email format, false otherwise</returns>
        public static bool IsValidEmail(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                // Use a regular expression for basic email validation
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(str);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all HTML tags from a string
        /// </summary>
        /// <param name="html">HTML string to clean</param>
        /// <returns>String with HTML tags removed</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // Remove HTML tags
            var result = Regex.Replace(html, @"<[^>]+>", "");
            
            // Replace HTML entities
            result = result.Replace("&nbsp;", " ")
                           .Replace("&amp;", "&")
                           .Replace("&lt;", "<")
                           .Replace("&gt;", ">")
                           .Replace("&quot;", "\"")
                           .Replace("&#39;", "'");
            
            return result;
        }

        /// <summary>
        /// Checks if a string contains only alphanumeric characters
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if the string is alphanumeric, false otherwise</returns>
        public static bool IsAlphanumeric(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return str.All(char.IsLetterOrDigit);
        }

        /// <summary>
        /// Checks if a string is null, empty, or consists only of whitespace
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if the string is null, empty, or whitespace; false otherwise</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Returns a default value if the string is null or empty
        /// </summary>
        /// <param name="str">String to check</param>
        /// <param name="defaultValue">Default value to return</param>
        /// <returns>The original string if not null or empty, otherwise the default value</returns>
        public static string DefaultIfNullOrEmpty(this string str, string defaultValue)
        {
            return string.IsNullOrEmpty(str) ? defaultValue : str;
        }

        /// <summary>
        /// Converts a string to a SecureString
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <returns>SecureString representation of the input string</returns>
        public static SecureString ToSecureString(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new SecureString();

            var secureString = new SecureString();
            foreach (char c in str)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }

        /// <summary>
        /// Checks if a string is a valid JSON format
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>True if the string is valid JSON, false otherwise</returns>
        public static bool IsValidJson(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            str = str.Trim();
            if ((str.StartsWith("{") && str.EndsWith("}")) || // Object
                (str.StartsWith("[") && str.EndsWith("]")))   // Array
            {
                try
                {
                    System.Text.Json.JsonDocument.Parse(str);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a string contains any of the specified substrings
        /// </summary>
        /// <param name="str">String to check</param>
        /// <param name="values">Substrings to check for</param>
        /// <param name="stringComparison">String comparison option</param>
        /// <returns>True if the string contains any of the substrings, false otherwise</returns>
        public static bool ContainsAny(this string str, IEnumerable<string> values, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || values == null)
                return false;

            return values.Any(value => !string.IsNullOrEmpty(value) && str.Contains(value, stringComparison));
        }

        /// <summary>
        /// Checks if a string contains all of the specified substrings
        /// </summary>
        /// <param name="str">String to check</param>
        /// <param name="values">Substrings to check for</param>
        /// <param name="stringComparison">String comparison option</param>
        /// <returns>True if the string contains all of the substrings, false otherwise</returns>
        public static bool ContainsAll(this string str, IEnumerable<string> values, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || values == null)
                return false;

            return values.All(value => !string.IsNullOrEmpty(value) && str.Contains(value, stringComparison));
        }

        /// <summary>
        /// Gets all occurrences of a pattern in a string
        /// </summary>
        /// <param name="str">String to search</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <returns>Collection of matches</returns>
        public static IEnumerable<string> GetMatches(this string str, string pattern)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(pattern))
                return Enumerable.Empty<string>();

            try
            {
                var regex = new Regex(pattern);
                var matches = regex.Matches(str);

                return matches.Cast<Match>().Select(m => m.Value);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Joins a collection of strings with a separator
        /// </summary>
        /// <typeparam name="T">Type of objects in the collection</typeparam>
        /// <param name="values">Collection of objects</param>
        /// <param name="separator">Separator to use</param>
        /// <returns>Joined string</returns>
        public static string JoinAsString<T>(this IEnumerable<T> values, string separator = ", ")
        {
            if (values == null)
                return string.Empty;

            return string.Join(separator, values);
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        /// <param name="str">String to reverse</param>
        /// <returns>Reversed string</returns>
        public static string Reverse(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Counts occurrences of a substring in a string
        /// </summary>
        /// <param name="str">String to search</param>
        /// <param name="substring">Substring to count</param>
        /// <param name="stringComparison">String comparison option</param>
        /// <returns>Number of occurrences</returns>
        public static int CountOccurrences(this string str, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = str.IndexOf(substring, index, stringComparison)) != -1)
            {
                count++;
                index += substring.Length;
            }

            return count;
        }

        /// <summary>
        /// Converts a string to a byte array using the specified encoding
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="encoding">Encoding to use (default: UTF8)</param>
        /// <returns>Byte array representation of the string</returns>
        public static byte[] ToByteArray(this string str, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(str))
                return Array.Empty<byte>();

            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Converts a byte array to a string using the specified encoding
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <param name="encoding">Encoding to use (default: UTF8)</param>
        /// <returns>String representation of the byte array</returns>
        public static string FromByteArray(this byte[] bytes, Encoding? encoding = null)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            encoding ??= Encoding.UTF8;
            return encoding.GetString(bytes);
        }
    }
}
