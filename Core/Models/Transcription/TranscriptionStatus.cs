namespace TrialWorld.Core.Models.Transcription
{
    /// <summary>
    /// Represents the status of a transcription job.
    /// </summary>
    public enum TranscriptionStatus
    {
        /// <summary>
        /// The transcription job has not been started.
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// The transcription job is queued for processing.
        /// </summary>
        Queued = 1,

        /// <summary>
        /// The transcription job is being processed.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// The transcription job has been completed successfully.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// The transcription job has failed with an error.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The transcription job has been cancelled.
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// The transcription job is uploading audio.
        /// </summary>
        UploadingAudio = 6,
        
        /// <summary>
        /// The transcription job is extracting audio from video.
        /// </summary>
        Extracting = 7,

        /// <summary>
        /// The transcription job status is unknown.
        /// </summary>
        Unknown = 99,

        RemovingSilence = 8,
        WaitingForTranscription = 9,
        Transcribing = 10,
        DownloadingResults = 11,
        Preprocessing = 12,
        Postprocessing = 13
    }
}
