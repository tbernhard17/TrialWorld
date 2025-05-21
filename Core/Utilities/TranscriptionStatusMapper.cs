using System;
using TrialWorld.Core.Models.Transcription;

using TrialWorld.Core.Interfaces;

namespace TrialWorld.Core.Utilities
{
    /// <summary>
    /// Central utility for mapping transcription statuses and phases between API, internal, and UI representations.
    /// </summary>
    public static class TranscriptionStatusMapper
    {
        /// <summary>
        /// Maps API or internal status string to a user-friendly display string for the UI.
        /// </summary>
        public static string ToDisplayString(string status, double silenceDetectionProgress = 0, double uploadProgress = 0, double transcribeProgress = 0)
        {
            switch (status)
            {
                case "Processing":
                    if (silenceDetectionProgress > 0 && silenceDetectionProgress < 100)
                        return "Removing Silence";
                    if (uploadProgress > 0 && uploadProgress < 100)
                        return "Uploading Audio";
                    if (transcribeProgress > 0 && transcribeProgress < 100)
                        return "Transcribing";
                    return "Processing";
                case "RemovingSilence":
                    return "Removing Silence";
                case "UploadingAudio":
                    return "Uploading Audio";
                case "WaitingForTranscription":
                    return "In Queue";
                case "Transcribing":
                    return "Transcribing";
                case "DownloadingResults":
                    return "Downloading";
                case "Preprocessing":
                    return "Pre-processing";
                case "Postprocessing":
                    return "Post-processing";
                case "SilenceDetection":
                    return "Detecting Silence";
                case "Uploading":
                    return "Uploading File";
                case "Submitted":
                    return "Submitted";
                case "Completed":
                    return "Completed";
                case "Failed":
                    return "Failed";
                case "Cancelled":
                    return "Cancelled";
                default:
                    return status;
            }
        }

        /// <summary>
        /// Maps an API status or phase to the internal enum, if needed.
        /// </summary>
        public static TranscriptionPhase ToTranscriptionPhase(string status)
        {
            switch (status)
            {
                case "Processing":
                    return TranscriptionPhase.Processing;
                case "RemovingSilence":
                    return TranscriptionPhase.Processing;
                case "UploadingAudio":
                case "Uploading":
                    return TranscriptionPhase.Uploading;
                case "WaitingForTranscription":
                    return TranscriptionPhase.Queued;
                case "Transcribing":
                    return TranscriptionPhase.Processing;
                case "DownloadingResults":
                    return TranscriptionPhase.Processing;
                case "Preprocessing":
                    return TranscriptionPhase.Processing;
                case "Postprocessing":
                    return TranscriptionPhase.Processing;
                case "SilenceDetection":
                    return TranscriptionPhase.Processing;
                case "Submitted":
                    return TranscriptionPhase.Queued;
                case "Completed":
                    return TranscriptionPhase.Completed;
                case "Failed":
                    return TranscriptionPhase.Failed;
                case "Cancelled":
                    return TranscriptionPhase.Cancelled;
                default:
                    return TranscriptionPhase.Queued;
            }
        }
    }
}
