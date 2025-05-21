using System;

namespace TrialWorld.Infrastructure.FFmpeg.Codecs
{
    /// <summary>
    /// Enum for common audio codecs used with FFmpeg
    /// </summary>
    public enum AudioCodec
    {
        /// <summary>MP3 audio codec</summary>
        LibMp3Lame,

        /// <summary>AAC audio codec</summary>
        Aac,

        /// <summary>Opus audio codec</summary>
        LibOpus,

        /// <summary>Vorbis audio codec</summary>
        LibVorbis,

        /// <summary>FLAC lossless audio codec</summary>
        Flac,

        /// <summary>PCM audio (WAV)</summary>
        Pcm,

        /// <summary>Copy the audio stream without re-encoding</summary>
        Copy
    }
}