using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TrialWorld.Core.Services
{
    public abstract class ResourceManagerBase : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, IDisposable> _resources = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _disposed;

        protected async Task<T> UseResourceAsync<T>(string key, Func<Task<T>> factory) where T : IDisposable
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ResourceManagerBase));

            await _lock.WaitAsync();
            try
            {
                if (_resources.TryGetValue(key, out var resource))
                    return (T)resource;

                var newResource = await factory();
                _resources.TryAdd(key, newResource);
                return newResource;
            }
            finally
            {
                _lock.Release();
            }
        }

        protected async Task ReleaseResourceAsync(string key)
        {
            await _lock.WaitAsync();
            try
            {
                if (_resources.TryRemove(key, out var resource))
                {
                    resource.Dispose();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            await _lock.WaitAsync();
            try
            {
                if (_disposed)
                    return;

                _disposed = true;
                foreach (var resource in _resources.Values)
                {
                    resource.Dispose();
                }
                _resources.Clear();
            }
            finally
            {
                _lock.Release();
                _lock.Dispose();
            }
        }
    }
}