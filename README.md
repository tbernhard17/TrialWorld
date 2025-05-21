# AssemblyAI Direct API Integration

## Overview

This project implements a production-ready direct integration with the AssemblyAI API for the WorldofTrials application. It replaces the deprecated AssemblyAI SDK with a robust, resilient direct API integration that follows C# and .NET best practices.

## Features

- **Direct API Integration**: Complete implementation of AssemblyAI's REST API endpoints
- **Enterprise-Grade Resilience**:
  - Exponential backoff with jitter for retries
  - Circuit breaker pattern to prevent cascading failures
  - Timeout policies with configurable thresholds
  - Respect for Retry-After headers
- **Comprehensive Logging**:
  - Structured logging with Serilog
  - Request/response correlation
  - Performance metrics
  - Detailed error reporting
- **Production-Ready Configuration**:
  - Environment-specific settings
  - Data annotation validation
  - Secure API key management
- **Dependency Injection**:
  - Proper service registration
  - Scoped HttpClient management
  - Interface-based design for testability

## Architecture

The implementation follows Clean Architecture principles with clear separation of concerns:

- **Configuration**: Strongly-typed options with validation
- **DTOs**: Data transfer objects for API requests and responses
- **Services**: Interface-based service implementations
- **Extensions**: DI registration and configuration extensions
- **Models**: Domain models for transcription data

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- AssemblyAI API key

### Configuration

Configure your AssemblyAI API key in `appsettings.json` or via environment variables:

```json
{
  "AssemblyAI": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://api.assemblyai.com/v2"
  }
}
```

Or set the environment variable:

```
ASSEMBLYAI__APIKEY=your-api-key-here
```

### Usage

#### Basic Usage

```csharp
// Get the service from DI
var transcriptionService = serviceProvider.GetRequiredService<IAssemblyAIDirectApiService>();

// Upload a file
var uploadResponse = await transcriptionService.UploadFileAsync("path/to/audio.mp3", cancellationToken);

// Submit for transcription
var transcriptionId = await transcriptionService.SubmitTranscriptionAsync(uploadResponse.UploadUrl, cancellationToken);

// Poll for completion
var transcript = await transcriptionService.GetTranscriptionAsync(transcriptionId, cancellationToken);
```

#### Diagnostic Tool

Run the diagnostic tool to test your AssemblyAI integration:

```
dotnet run --project AssemblyAIDiagnosticTool.csproj
```

## Advanced Configuration

The implementation supports extensive configuration options:

```json
{
  "AssemblyAI": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://api.assemblyai.com/v2",
    "TimeoutSeconds": 600,
    "PollingIntervalMs": 5000,
    "MaxRetryAttempts": 3,
    "UploadChunkSizeBytes": 5242880,
    "EnableDetailedLogging": true,
    "RetryBackoffExponent": 2,
    "InitialRetryDelayMs": 1000,
    "MaxRetryDelayMs": 30000,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerSamplingDurationSeconds": 30,
    "CircuitBreakerDurationSeconds": 60,
    "RequestTimeoutSeconds": 30
  }
}
```

## Error Handling

The implementation includes comprehensive error handling:

- **Transient Errors**: Automatically retried with exponential backoff
- **Rate Limiting**: Respects Retry-After headers
- **Circuit Breaking**: Prevents cascading failures
- **Timeout Handling**: Configurable timeouts for all operations
- **Detailed Logging**: Structured logging of all errors

## Performance Considerations

- **Chunked Uploads**: Large files are uploaded in configurable chunks
- **Asynchronous Operations**: All operations are fully asynchronous
- **Connection Pooling**: Proper HttpClient management
- **Cancellation Support**: All operations support cancellation tokens

## Security

- **API Key Management**: Secure handling of API keys
- **HTTPS**: All communication over HTTPS
- **Input Validation**: Proper validation of all inputs

## Logging

The implementation uses Serilog for structured logging:

- **Console Logging**: JSON-formatted logs to console
- **File Logging**: Rolling file logs with retention policy
- **Correlation IDs**: Request/response correlation
- **Performance Metrics**: Timing information for all operations

## Best Practices

This implementation follows C# and .NET best practices:

- **Async/Await**: Proper async method naming and patterns
- **Dependency Injection**: Constructor injection for dependencies
- **Options Pattern**: Strongly-typed configuration
- **Interface Segregation**: Clean interfaces
- **Proper Disposal**: IDisposable implementation where needed
- **XML Documentation**: Comprehensive XML documentation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

Copyright © TrialWorld 2025
