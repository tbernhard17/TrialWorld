using System;

namespace TrialWorld.Contracts.Logging
{
    /// <summary>
    /// Defines constants for structured logging related to AssemblyAI operations.
    /// </summary>
    public static class AssemblyAILoggingConstants
    {
        // Event ID ranges for different operation categories
        public static class EventIds
        {
            // API Client operations (1000-1099)
            public const int ApiClientInitialization = 1000;
            public const int ApiRequestStarted = 1001;
            public const int ApiRequestCompleted = 1002;
            public const int ApiRequestFailed = 1003;
            public const int ApiRetryAttempt = 1004;
            public const int ApiRateLimitExceeded = 1005;
            
            // File operations (1100-1199)
            public const int FileValidationStarted = 1100;
            public const int FileValidationCompleted = 1101;
            public const int FileValidationFailed = 1102;
            public const int FileUploadStarted = 1110;
            public const int FileUploadProgress = 1111;
            public const int FileUploadCompleted = 1112;
            public const int FileUploadFailed = 1113;
            
            // Transcription operations (1200-1299)
            public const int TranscriptionRequestStarted = 1200;
            public const int TranscriptionRequestSubmitted = 1201;
            public const int TranscriptionStatusCheck = 1210;
            public const int TranscriptionCompleted = 1220;
            public const int TranscriptionFailed = 1221;
            public const int TranscriptionCancelled = 1230;
            
            // Content analysis operations (1300-1399)
            public const int ContentAnalysisStarted = 1300;
            public const int ContentAnalysisCompleted = 1301;
            public const int ContentAnalysisFailed = 1302;
        }
        
        // Log property names for structured logging
        public static class Properties
        {
            public const string CorrelationId = "CorrelationId";
            public const string RequestId = "RequestId";
            public const string TranscriptionId = "TranscriptionId";
            public const string MediaId = "MediaId";
            public const string FileName = "FileName";
            public const string FilePath = "FilePath";
            public const string FileSize = "FileSize";
            public const string FileExtension = "FileExtension";
            public const string StatusCode = "StatusCode";
            public const string ElapsedMilliseconds = "ElapsedMilliseconds";
            public const string RetryAttempt = "RetryAttempt";
            public const string MaxRetries = "MaxRetries";
            public const string ProgressPercentage = "ProgressPercentage";
            public const string ApiEndpoint = "ApiEndpoint";
            public const string RequestMethod = "RequestMethod";
            public const string RequestHeaders = "RequestHeaders";
            public const string RequestBody = "RequestBody";
            public const string ResponseHeaders = "ResponseHeaders";
            public const string ResponseBody = "ResponseBody";
            public const string ErrorCode = "ErrorCode";
            public const string ErrorMessage = "ErrorMessage";
        }
        
        // Category names for logging
        public static class Categories
        {
            public const string ApiClient = "TrialWorld.AssemblyAI.ApiClient";
            public const string FileOperations = "TrialWorld.AssemblyAI.FileOperations";
            public const string Transcription = "TrialWorld.AssemblyAI.Transcription";
            public const string ContentAnalysis = "TrialWorld.AssemblyAI.ContentAnalysis";
        }
    }
}