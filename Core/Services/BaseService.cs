using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace TrialWorld.Core.Services
{
    public abstract class BaseService<T, TOptions> : IDisposable where TOptions : class, new()
    {
        protected readonly ILogger<T> Logger;
        protected readonly TOptions Options;

        protected BaseService(ILogger<T> logger, IOptions<TOptions> options)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Options = options?.Value ?? new TOptions();
            ValidateOptions(Options);
        }

        protected virtual void ValidateOptions(TOptions options) { }

        protected void LogOperation(string operation, Action action)
        {
            try
            {
                Logger.LogInformation($"Starting {operation}");
                action();
                Logger.LogInformation($"Completed {operation}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during {operation}");
                throw;
            }
        }

        protected async Task LogOperationAsync(string operation, Func<Task> action)
        {
            try
            {
                Logger.LogInformation($"Starting {operation}");
                await action();
                Logger.LogInformation($"Completed {operation}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during {operation}");
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            // Base implementation does nothing
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}