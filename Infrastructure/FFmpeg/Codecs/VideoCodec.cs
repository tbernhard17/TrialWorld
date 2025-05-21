using System;

namespace TrialWorld.Infrastructure.FFmpeg.Codecs
{
    /// <summary>
    /// Enum for common video codecs used with FFmpeg
    /// </summary>
    public enum VideoCodec
    {
        /// <summary>H.264 / AVC codec (libx264)</summary>
        LibX264,

        /// <summary>H.265 / HEVC codec (libx265)</summary>
        LibX265,

        /// <summary>VP9 codec</summary>
        LibVpx,

        /// <summary>AV1 codec</summary>
        LibAom,

        /// <summary>MPEG-4 codec</summary>
        Mpeg4,

        /// <summary>ProRes codec</summary>
        ProRes,

        /// <summary>FFV1 lossless codec</summary>
        FFV1,

        /// <summary>Copy the video stream without re-encoding</summary>
        Copy,

        /// <summary>MJPEG codec</summary>
        Mjpeg,

        /// <summary>H.264 codec (NVIDIA NVENC)</summary>
        H264Nvenc,

        /// <summary>H.265 / HEVC codec (NVIDIA NVENC)</summary>
        HevcNvenc,

        /// <summary>H.264 codec (Intel QuickSync Video)</summary>
        H264Qsv,

        /// <summary>H.265 / HEVC codec (Intel QuickSync Video)</summary>
        HevcQsv,

        /// <summary>H.264 codec (AMD Advanced Media Framework)</summary>
        H264Amf,

        /// <summary>H.265 / HEVC codec (AMD Advanced Media Framework)</summary>
        HevcAmf
    }
}