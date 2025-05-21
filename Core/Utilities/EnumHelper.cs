using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Core.Utilities
{
    /// <summary>
    /// Provides utility methods for working with enumeration types
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets all values of an enum type
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Collection of enum values</returns>
        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Gets the display name of an enum value
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>Display name from DisplayAttribute, or the enum value name if no attribute is found</returns>
        public static string GetDisplayName(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
                return displayAttribute.Name ?? value.ToString();

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                return descriptionAttribute.Description;

            return value.ToString();
        }

        /// <summary>
        /// Gets the description of an enum value
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>Description from DescriptionAttribute, or the display name if no description is found</returns>
        public static string GetDescription(Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            FieldInfo? field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                return descriptionAttribute.Description;

            var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
                return displayAttribute.Description ?? displayAttribute.Name ?? value.ToString();

            return value.ToString();
        }

        /// <summary>
        /// Gets an enum value from its string representation
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">String representation of enum value</param>
        /// <param name="ignoreCase">Whether to ignore case when parsing</param>
        /// <returns>Enum value or default if parsing fails</returns>
        public static T? Parse<T>(string value, bool ignoreCase = true) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
                return default;

            if (Enum.TryParse<T>(value, ignoreCase, out T result))
                return result;

            return default;
        }

        /// <summary>
        /// Tries to parse a string into an enum value
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">String representation of enum value</param>
        /// <param name="result">Parsed enum value</param>
        /// <param name="ignoreCase">Whether to ignore case when parsing</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParse<T>(string value, out T result, bool ignoreCase = true) where T : struct, Enum
        {
            result = default;
            
            if (string.IsNullOrEmpty(value))
                return false;

            return Enum.TryParse(value, ignoreCase, out result);
        }

        /// <summary>
        /// Gets an enum value from its display name or description
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="displayName">Display name or description to look for</param>
        /// <param name="ignoreCase">Whether to ignore case when comparing</param>
        /// <returns>Enum value or default if not found</returns>
        public static T? ParseFromDisplayName<T>(string displayName, bool ignoreCase = true) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(displayName))
                return default;

            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (T value in GetValues<T>())
            {
                if (string.Equals(GetDisplayName(value as Enum), displayName, comparisonType) ||
                    string.Equals(GetDescription(value as Enum), displayName, comparisonType))
                {
                    return value;
                }
            }

            return default;
        }

        /// <summary>
        /// Checks if an enum has a specific flag set
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">Enum value to check</param>
        /// <param name="flag">Flag to check for</param>
        /// <returns>True if the flag is set, false otherwise</returns>
        public static bool HasFlag<T>(T value, T flag) where T : Enum
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (flag == null)
                throw new ArgumentNullException(nameof(flag));

            Type underlyingType = Enum.GetUnderlyingType(typeof(T));

            if (underlyingType == typeof(int))
            {
                return ((int)(object)value & (int)(object)flag) == (int)(object)flag;
            }
            else if (underlyingType == typeof(long))
            {
                return ((long)(object)value & (long)(object)flag) == (long)(object)flag;
            }
            else if (underlyingType == typeof(short))
            {
                return ((short)(object)value & (short)(object)flag) == (short)(object)flag;
            }
            else if (underlyingType == typeof(byte))
            {
                return ((byte)(object)value & (byte)(object)flag) == (byte)(object)flag;
            }
            else if (underlyingType == typeof(uint))
            {
                return ((uint)(object)value & (uint)(object)flag) == (uint)(object)flag;
            }
            else if (underlyingType == typeof(ulong))
            {
                return ((ulong)(object)value & (ulong)(object)flag) == (ulong)(object)flag;
            }
            else if (underlyingType == typeof(ushort))
            {
                return ((ushort)(object)value & (ushort)(object)flag) == (ushort)(object)flag;
            }
            else if (underlyingType == typeof(sbyte))
            {
                return ((sbyte)(object)value & (sbyte)(object)flag) == (sbyte)(object)flag;
            }

            return value.HasFlag(flag);
        }

        /// <summary>
        /// Gets all enum values with a specific attribute
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <typeparam name="TAttribute">Attribute type</typeparam>
        /// <returns>Collection of enum values with the specified attribute</returns>
        public static IEnumerable<T> GetValuesWithAttribute<T, TAttribute>() where T : Enum where TAttribute : Attribute
        {
            foreach (T value in GetValues<T>())
            {
                FieldInfo? field = typeof(T).GetField(value.ToString());
                if (field != null && field.GetCustomAttribute<TAttribute>() != null)
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Creates a dictionary mapping enum values to their display names
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Dictionary of enum values and their display names</returns>
        public static Dictionary<T, string> ToDictionary<T>() where T : Enum
        {
            return GetValues<T>().ToDictionary(
                value => value,
                value => GetDisplayName(value)
            );
        }

        /// <summary>
        /// Gets a list of key-value pairs for use in UI dropdowns or selection lists
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="useDescription">Whether to use descriptions instead of display names</param>
        /// <returns>List of key-value pairs for the enum values</returns>
        public static List<KeyValuePair<T, string>> GetDropdownList<T>(bool useDescription = false) where T : Enum
        {
            return GetValues<T>().Select(value => new KeyValuePair<T, string>(
                value,
                useDescription ? GetDescription(value) : GetDisplayName(value)
            )).ToList();
        }

        /// <summary>
        /// Gets a list of key-value pairs for numeric enum values
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="useDescription">Whether to use descriptions instead of display names</param>
        /// <returns>List of numeric key-value pairs for the enum values</returns>
        public static List<KeyValuePair<int, string>> GetNumericDropdownList<T>(bool useDescription = false) where T : Enum
        {
            return GetValues<T>().Select(value => new KeyValuePair<int, string>(
                Convert.ToInt32(value),
                useDescription ? GetDescription(value) : GetDisplayName(value)
            )).ToList();
        }

        /// <summary>
        /// Gets a specific attribute from an enum value
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <typeparam name="TAttribute">Attribute type</typeparam>
        /// <param name="value">Enum value to get the attribute from</param>
        /// <returns>The attribute if found, null otherwise</returns>
        public static TAttribute? GetAttribute<T, TAttribute>(T value) where T : Enum where TAttribute : Attribute
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            FieldInfo? field = typeof(T).GetField(value.ToString());
            if (field == null)
                return null;

            return field.GetCustomAttribute<TAttribute>();
        }
    }
}
