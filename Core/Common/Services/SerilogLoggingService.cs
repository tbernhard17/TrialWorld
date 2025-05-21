using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using TrialWorld.Core.Common.Interfaces;

namespace TrialWorld.Core.Common.Services
{
    /// <summary>
    /// Serilog implementation of the logging service
    /// </summary>
    public class SerilogLoggingService : ILoggingService
    {
        private readonly Serilog.ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogLoggingService"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public SerilogLoggingService(IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            string logFilePath = configuration["Logging:FilePath"] ?? "logs/trialworld-.log";
            
            loggerConfig.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31);

            _logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogLoggingService"/> class
        /// with an existing logger.
        /// </summary>
        /// <param name="logger">The Serilog logger instance</param>
        private SerilogLoggingService(Serilog.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void LogTrace(string message, params object?[] args)
        {
            _logger.Verbose(message, args);
        }

        /// <inheritdoc />
        public void LogDebug(string message, params object?[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc />
        public void LogInfo(string message, params object?[] args)
        {
            _logger.Information(message, args);
        }

        /// <inheritdoc />
        public void LogWarning(string message, params object?[] args)
        {
            _logger.Warning(message, args);
        }

        /// <inheritdoc />
        public void LogError(string message, params object?[] args)
        {
            _logger.Error(message, args);
        }

        /// <inheritdoc />
        public void LogError(Exception exception, string message, params object?[] args)
        {
           if (args == null || args.Length == 0)
           {
               // Serilog doesn't handle null or empty args gracefully in this overload
               _logger.Error(exception, message);
           }
           else
           {
               _logger.Error(exception, message, args);
           }
        }

        /// <inheritdoc />
        public void LogCritical(string message, params object?[] args)
        {
            _logger.Fatal(message, args);
        }

        /// <inheritdoc />
        public void LogCritical(Exception exception, string message, params object?[] args)
        {
           if (args == null || args.Length == 0)
           {
               // Serilog doesn't handle null or empty args gracefully in this overload
               _logger.Fatal(exception, message);
           }
           else
           {
               _logger.Fatal(exception, message, args);
           }
        }

        /// <inheritdoc />
        public ILoggingService ForContext<T>()
        {
            return new SerilogLoggingService(_logger.ForContext<T>());
        }

        /// <inheritdoc />
        public ILoggingService ForContext(Type type)
        {
            return new SerilogLoggingService(_logger.ForContext(type));
        }
    }
}
