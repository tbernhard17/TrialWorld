using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.Logging;
using TrialWorld.Core.Interfaces;
using TrialWorld.Core.Models;
using CoreMediaInfo = TrialWorld.Core.Models.MediaInfo;
using CorePlaybackState = TrialWorld.Core.Interfaces.PlaybackState;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TrialWorld.Infrastructure.MediaPlayback
{
    // Helper classes
    public class MediaPlayerEventArgs : EventArgs
    {
        public MediaInfo? Media { get; set; }
    }

    public class PositionChangedEventArgs : EventArgs
    {
        public double Position { get; set; }
        public double Duration { get; set; }
    }
// Content moved from Infrastructure/FFmpegMediaPlayer.cs
// Preserving all implementation details
    public class MediaImage : IMediaImage
    {
        private readonly byte[] _imageData;
        private readonly int _width;
        private readonly int _height;

        public int Width => _width;
        public int Height => _height;

        public MediaImage(byte[] imageData)
        {
            _imageData = imageData;

            try
            {
                // Extract dimensions from image data
                using var ms = new MemoryStream(imageData);
                var decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
                var frame = decoder.Frames[0];
                _width = frame.PixelWidth;
                _height = frame.PixelHeight;
            }
            catch
            {
                _width = 0;
                _height = 0;
            }
        }

        public byte[] GetPixelData()
        {
            return _imageData;
        }
    }
}