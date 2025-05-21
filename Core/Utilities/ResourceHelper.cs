using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Utilities
{
    /// <summary>
    /// Provides utilities for managing disposable resources
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Safely disposes a resource and sets it to null
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to dispose</param>
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

        /// <summary>
        /// Safely disposes an async resource and sets it to null
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        /// <param name="resource">Resource to dispose</param>
        /// <returns>Null resource after disposing</returns>
        public static async Task<T?> SafeDisposeAsync<T>(T? resource) where T : class, IAsyncDisposable
        {
            if (resource != null)
            {
                await resource.DisposeAsync();
                resource = null;
            }
            return resource;
        }

        /// <summary>
        /// Disposes all resources in a collection
        /// </summary>
        /// <typeparam name="T">Type of resources</typeparam>
        /// <param name="resources">Collection of resources to dispose</param>
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

        /// <summary>
        /// Disposes all async resources in a collection
        /// </summary>
        /// <typeparam name="T">Type of resources</typeparam>
        /// <param name="resources">Collection of resources to dispose</param>
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
