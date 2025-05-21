using System;

namespace TrialWorld.Infrastructure.FFmpeg.Codecs
{
    /// <summary>
    /// Extension methods for working with codec enums
    /// </summary>
    public static class CodecExtensions
    {
        /// <summary>
        /// Converts a VideoCodec enum to its FFmpeg string representation
        /// </summary>
        /// <param name="codec">The video codec</param>
        /// <returns>FFmpeg codec string</returns>
        public static string GetCodecName(this VideoCodec codec)
        {
            return codec switch
            {
                VideoCodec.LibX264 => "libx264",
                VideoCodec.LibX265 => "libx265",
                VideoCodec.LibVpx => "libvpx-vp9",
                VideoCodec.LibAom => "libaom-av1",
                VideoCodec.Mpeg4 => "mpeg4",
                VideoCodec.Mjpeg => "mjpeg",
                VideoCodec.H264Nvenc => "h264_nvenc",
                VideoCodec.HevcNvenc => "hevc_nvenc",
                VideoCodec.H264Qsv => "h264_qsv",
                VideoCodec.HevcQsv => "hevc_qsv",
                VideoCodec.H264Amf => "h264_amf",
                VideoCodec.HevcAmf => "hevc_amf",
                _ => throw new ArgumentOutOfRangeException(nameof(codec), codec, "Unknown video codec")
            };
        }

        /// <summary>
        /// Converts an AudioCodec enum to its FFmpeg string representation
        /// </summary>
        /// <param name="codec">The audio codec</param>
        /// <returns>FFmpeg codec string</returns>
        public static string GetCodecName(this AudioCodec codec)
        {
            return codec switch
            {
                AudioCodec.LibMp3Lame => "libmp3lame",
                AudioCodec.Aac => "aac",
                AudioCodec.Flac => "flac",
                AudioCodec.LibOpus => "libopus",
                AudioCodec.LibVorbis => "libvorbis",
                AudioCodec.Pcm => "pcm_s16le",
                AudioCodec.Copy => "copy",
                _ => throw new ArgumentOutOfRangeException(nameof(codec), codec, "Unknown audio codec")
            };
        }
    }
}