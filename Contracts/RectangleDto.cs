using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Represents a rectangle with position and size information in an image/video frame
    /// </summary>
    public class RectangleDto
    {
        /// <summary>
        /// X-coordinate of the top-left corner
        /// </summary>
        public float X { get; set; }
        
        /// <summary>
        /// Y-coordinate of the top-left corner
        /// </summary>
        public float Y { get; set; }
        
        /// <summary>
        /// Width of the rectangle
        /// </summary>
        public float Width { get; set; }
        
        /// <summary>
        /// Height of the rectangle
        /// </summary>
        public float Height { get; set; }
        
        /// <summary>
        /// Timestamp when this rectangle was detected
        /// </summary>
        public TimeSpan? Timestamp { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RectangleDto()
        {
        }
        
        /// <summary>
        /// Creates a rectangle with the specified dimensions
        /// </summary>
        public RectangleDto(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates a rectangle with the specified dimensions
        /// </summary>
        public RectangleDto(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        /// <summary>
        /// Creates a rectangle with the specified dimensions and timestamp
        /// </summary>
        public RectangleDto(int x, int y, int width, int height, TimeSpan timestamp)
            : this(x, y, width, height)
        {
            Timestamp = timestamp;
        }

        /// <summary>
        /// Creates a new rectangle with the specified dimensions and timestamp
        /// </summary>
        public RectangleDto(float x, float y, float width, float height, TimeSpan timestamp)
            : this(x, y, width, height)
        {
            Timestamp = timestamp;
        }
    }
}