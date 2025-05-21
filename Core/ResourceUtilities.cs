using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public static class ResourceManager
    {
        public static void SafeDispose<T>(ref T? resource) where T : class, IDisposable
        {
            if (resource != null)
            {
                try
                {
                    resource.Dispose();
                }
                catch (Exception ex)
                {
                    // Log the exception if needed
                    System.Diagnostics.Debug.WriteLine($"Error disposing resource: {ex.Message}");
                }
                finally
                {
                    resource = null;
                }
            }
        }

        public static async Task<T?> SafeDisposeAsync<T>(T? resource) where T : class, IAsyncDisposable
        {
            if (resource != null)
            {
                await resource.DisposeAsync();
                resource = null;
            }
            return resource;
        }

        public static void DisposeAll<T>(IEnumerable<T> resources) where T : IDisposable
        {
            foreach (var resource in resources)
            {
                if (resource != null)
                {
                    try
                    {
                        resource.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if needed
                        System.Diagnostics.Debug.WriteLine($"Error disposing resource: {ex.Message}");
                    }
                }
            }
        }

        public static async Task DisposeAllAsync<T>(IEnumerable<T> resources) where T : IAsyncDisposable
        {
            foreach (var resource in resources)
            {
                if (resource != null)
                {
                    try
                    {
                        await resource.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if needed
                        System.Diagnostics.Debug.WriteLine($"Error disposing resource asynchronously: {ex.Message}");
                    }
                }
            }
        }
    }
}