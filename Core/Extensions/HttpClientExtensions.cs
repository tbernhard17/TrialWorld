using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TrialWorld.Core.Models.Common;

namespace TrialWorld.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for HttpClient and related operations.
    /// </summary>
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
            // Add other default options as needed
        };

        /// <summary>
        /// Sends a POST request with JSON content asynchronously.
        /// </summary>
        public static async Task<OperationResult<TResponse>> PostAsJsonAsync<TRequest, TResponse>(
            this HttpClient client,
            string requestUri,
            TRequest value,
            ILogger? logger = null,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, options ?? DefaultJsonSerializerOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                logger?.LogDebug("Sending POST request to {RequestUri} with body: {JsonBody}", requestUri, json);
                var response = await client.PostAsync(requestUri, content, cancellationToken);

                // Process the response using the helper method
                return await ProcessResponseAsync<TResponse>(response, logger, options ?? DefaultJsonSerializerOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                // Use centralized exception handler
                return HandleRequestException<TResponse>(ex, logger, requestUri, "POST");
            }
        }

        /// <summary>
        /// Sends a GET request asynchronously and deserializes the JSON response.
        /// </summary>
        public static async Task<OperationResult<TResponse>> GetFromJsonAsync<TResponse>(
            this HttpClient client,
            string requestUri,
            ILogger? logger = null,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger?.LogDebug("Sending GET request to {RequestUri}", requestUri);
                var response = await client.GetAsync(requestUri, cancellationToken);

                // Process the response using the helper method
                return await ProcessResponseAsync<TResponse>(response, logger, options ?? DefaultJsonSerializerOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                // Use centralized exception handler
                return HandleRequestException<TResponse>(ex, logger, requestUri, "GET");
            }
        }

        /// <summary>
        /// Processes the HttpResponseMessage, handling status codes and deserialization.
        /// </summary>
        private static async Task<OperationResult<T>> ProcessResponseAsync<T>(
            HttpResponseMessage response,
            ILogger? logger,
            JsonSerializerOptions jsonOptions,
            CancellationToken cancellationToken)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    if (responseStream.Length == 0)
                    {
                        logger?.LogWarning("Received successful response but content stream was empty.");
                        return OperationResult<T>.Failure("EmptyResponse", "Successful response received but content was empty.");
                    }

                    var resultData = await JsonSerializer.DeserializeAsync<T>(responseStream, jsonOptions, cancellationToken);
                    if (resultData == null)
                    {
                        logger?.LogError("JSON deserialization resulted in null for type {TypeName}", typeof(T).Name);
                        return OperationResult<T>.Failure("DeserializationNull", $"Failed to deserialize response content into {typeof(T).Name}. Result was null.");
                    }
                    logger?.LogDebug("Successfully received and deserialized response of type {TypeName}", typeof(T).Name);
                    return OperationResult<T>.SuccessResult(resultData);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger?.LogError("HTTP request failed with status code {StatusCode}. Reason: {ReasonPhrase}. Content: {ErrorContent}",
                        response.StatusCode, response.ReasonPhrase, errorContent);
                    return OperationResult<T>.Failure($"HttpError_{(int)response.StatusCode}",
                        $"Request failed: {response.ReasonPhrase}. Details: {errorContent}");
                }
            }
            catch (JsonException ex)
            {
                logger?.LogError(ex, "JSON deserialization error while processing response");
                return OperationResult<T>.Failure("JsonError", ex.Message);
            }
            catch (OperationCanceledException ex)
            {
                logger?.LogWarning(ex, "Operation cancelled while processing response");
                return OperationResult<T>.Failure("Cancelled", "Operation was cancelled during response processing.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unexpected error while processing response");
                return OperationResult<T>.Failure("ResponseProcessingError", ex.Message);
            }
        }

        /// <summary>
        /// Sends a GET request and returns the raw HttpResponseMessage.
        /// </summary>
        public static async Task<OperationResult<HttpResponseMessage>> GetAsyncRaw(
            this HttpClient client,
            string requestUri,
            Action<HttpRequestHeaders>? configureHeaders = null,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                configureHeaders?.Invoke(request.Headers);

                logger?.LogDebug("Sending raw GET request to {RequestUri}", requestUri);
                var response = await client.SendAsync(request, cancellationToken);

                return OperationResult<HttpResponseMessage>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                // Use centralized exception handler
                return HandleRequestException<HttpResponseMessage>(ex, logger, requestUri, "GET (Raw)");
            }
        }

        /// <summary>
        /// Sends a POST request with form URL encoded content asynchronously.
        /// </summary>
        public static async Task<OperationResult<TResponse>> PostFormUrlEncodedAsync<TResponse>(
            this HttpClient client,
            string requestUri,
            IEnumerable<KeyValuePair<string, string>> data,
            ILogger? logger = null,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var content = new FormUrlEncodedContent(data);

                logger?.LogDebug("Sending POST request with form URL encoded data to {RequestUri}", requestUri);
                var response = await client.PostAsync(requestUri, content, cancellationToken);

                // Process the response using the helper method
                return await ProcessResponseAsync<TResponse>(response, logger, options ?? DefaultJsonSerializerOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                // Use centralized exception handler
                return HandleRequestException<TResponse>(ex, logger, requestUri, "POST (Form)");
            }
        }

        // Centralized exception handler for common request errors
        private static OperationResult<TResponse> HandleRequestException<TResponse>(
            Exception ex,
            ILogger? logger,
            string requestUri,
            string operationType)
        {
            switch (ex)
            {
                case HttpRequestException httpEx:
                    logger?.LogError(httpEx, "HTTP request error during {OperationType} to {RequestUri}", operationType, requestUri);
                    return OperationResult<TResponse>.Failure("HttpRequestError", httpEx.Message);
                case JsonException jsonEx: // Catch JsonException during request setup if possible (though less likely here)
                    logger?.LogError(jsonEx, "JSON error during {OperationType} setup for {RequestUri}", operationType, requestUri);
                    return OperationResult<TResponse>.Failure("JsonError", jsonEx.Message);
                case OperationCanceledException ocEx:
                    logger?.LogWarning(ocEx, "{OperationType} request cancelled for {RequestUri}", operationType, requestUri);
                    return OperationResult<TResponse>.Failure("Cancelled", "Operation was cancelled.");
                default:
                    logger?.LogError(ex, "Unexpected error during {OperationType} to {RequestUri}", operationType, requestUri);
                    return OperationResult<TResponse>.Failure("UnexpectedError", ex.Message);
            }
        }
    }
}