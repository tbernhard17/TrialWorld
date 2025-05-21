using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Core
{
    /// <summary>
    /// Provides utility methods for standardized error handling throughout the application
    /// </summary>
    public static class ErrorHandling
    {
        /// <summary>
        /// Executes an action with standardized exception handling
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="logger">Logger for error logging</param>
        /// <param name="errorMessage">Error message to log</param>
        /// <param name="rethrow">Whether to rethrow the exception</param>
        /// <param name="callerMemberName">Name of the calling method (automatically populated)</param>
        /// <param name="callerFilePath">Path to the calling file (automatically populated)</param>
        /// <param name="callerLineNumber">Line number in the calling file (automatically populated)</param>
        /// <returns>True if the action executed successfully, false otherwise</returns>
        public static bool TryCatch(
            Action action,
            ILogger logger,
            string? errorMessage = null,
            bool rethrow = false,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, logger, errorMessage, callerMemberName, callerFilePath, callerLineNumber);
                
                if (rethrow)
                    throw;
                    
                return false;
            }
        }

        /// <summary>
        /// Executes a function with standardized exception handling
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Function to execute</param>
        /// <param name="logger">Logger for error logging</param>
        /// <param name="defaultValue">Default value to return if an exception occurs</param>
        /// <param name="errorMessage">Error message to log</param>
        /// <param name="rethrow">Whether to rethrow the exception</param>
        /// <param name="callerMemberName">Name of the calling method (automatically populated)</param>
        /// <param name="callerFilePath">Path to the calling file (automatically populated)</param>
        /// <param name="callerLineNumber">Line number in the calling file (automatically populated)</param>
        /// <returns>Result of the function or the default value if an exception occurs</returns>
        public static T TryCatch<T>(
            Func<T> func,
            ILogger logger,
            T defaultValue = default!,
            string? errorMessage = null,
            bool rethrow = false,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                LogException(ex, logger, errorMessage, callerMemberName, callerFilePath, callerLineNumber);
                
                if (rethrow)
                    throw;
                    
                return defaultValue;
            }
        }

        /// <summary>
        /// Executes an async function with standardized exception handling
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Async function to execute</param>
        /// <param name="logger">Logger for error logging</param>
        /// <param name="defaultValue">Default value to return if an exception occurs</param>
        /// <param name="errorMessage">Error message to log</param>
        /// <param name="rethrow">Whether to rethrow the exception</param>
        /// <param name="callerMemberName">Name of the calling method (automatically populated)</param>
        /// <param name="callerFilePath">Path to the calling file (automatically populated)</param>
        /// <param name="callerLineNumber">Line number in the calling file (automatically populated)</param>
        /// <returns>Result of the function or the default value if an exception occurs</returns>
        public static async Task<T> TryCatchAsync<T>(
            Func<Task<T>> func,
            ILogger logger,
            T defaultValue = default!,
            string? errorMessage = null,
            bool rethrow = false,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                LogException(ex, logger, errorMessage, callerMemberName, callerFilePath, callerLineNumber);
                
                if (rethrow)
                    throw;
                    
                return defaultValue;
            }
        }

        /// <summary>
        /// Executes an async action with standardized exception handling
        /// </summary>
        /// <param name="func">Async action to execute</param>
        /// <param name="logger">Logger for error logging</param>
        /// <param name="errorMessage">Error message to log</param>
        /// <param name="rethrow">Whether to rethrow the exception</param>
        /// <param name="callerMemberName">Name of the calling method (automatically populated)</param>
        /// <param name="callerFilePath">Path to the calling file (automatically populated)</param>
        /// <param name="callerLineNumber">Line number in the calling file (automatically populated)</param>
        /// <returns>True if the action executed successfully, false otherwise</returns>
        public static async Task<bool> TryCatchAsync(
            Func<Task> func,
            ILogger logger,
            string? errorMessage = null,
            bool rethrow = false,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            try
            {
                await func();
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, logger, errorMessage, callerMemberName, callerFilePath, callerLineNumber);
                
                if (rethrow)
                    throw;
                    
                return false;
            }
        }

        /// <summary>
        /// Logs an exception with standardized formatting
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="logger">Logger to use</param>
        /// <param name="customMessage">Custom message to include in the log</param>
        /// <param name="callerMemberName">Name of the calling method</param>
        /// <param name="callerFilePath">Path to the calling file</param>
        /// <param name="callerLineNumber">Line number in the calling file</param>
        public static void LogException(
            Exception exception,
            ILogger logger,
            string? customMessage = null,
            string callerMemberName = "",
            string callerFilePath = "",
            int callerLineNumber = 0)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            // Prepare context information
            var context = new Dictionary<string, string>
            {
                ["Method"] = callerMemberName,
                ["File"] = System.IO.Path.GetFileName(callerFilePath),
                ["Line"] = callerLineNumber.ToString()
            };

            // Create log message
            var message = string.IsNullOrEmpty(customMessage)
                ? $"Exception in {callerMemberName}"
                : customMessage;

            // Log the exception
            logger.LogError(exception, "{Message} [{Method}:{File}:{Line}]", message, context["Method"], context["File"], context["Line"]);
        }

        /// <summary>
        /// Checks if an argument is not null
        /// </summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="argument">Argument to check</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <returns>The argument if not null</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
        public static T ThrowIfNull<T>(this T argument, string paramName)
        {
            if (argument == null)
                throw new ArgumentNullException(paramName);

            return argument;
        }

        /// <summary>
        /// Checks if a string argument is not null or empty
        /// </summary>
        /// <param name="argument">String argument to check</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <returns>The string if not null or empty</returns>
        /// <exception cref="ArgumentException">Thrown when the string is null or empty</exception>
        public static string ThrowIfNullOrEmpty(this string argument, string paramName)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentException("Value cannot be null or empty", paramName);

            return argument;
        }

        /// <summary>
        /// Checks if a collection is not null or empty
        /// </summary>
        /// <typeparam name="T">Type of items in the collection</typeparam>
        /// <param name="collection">Collection to check</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <returns>The collection if not null or empty</returns>
        /// <exception cref="ArgumentException">Thrown when the collection is null or empty</exception>
        public static IEnumerable<T> ThrowIfNullOrEmpty<T>(this IEnumerable<T> collection, string paramName)
        {
            if (collection == null)
                throw new ArgumentNullException(paramName);

            if (!collection.Any())
                throw new ArgumentException("Collection cannot be empty", paramName);

            return collection;
        }

        /// <summary>
        /// Checks if a value is within a specified range
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="value">Value to check</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <returns>The value if within the range</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the range</exception>
        public static T ThrowIfOutOfRange<T>(this T value, T min, T max, string paramName) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}");

            return value;
        }
    }
}