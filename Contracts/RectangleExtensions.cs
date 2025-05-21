using System;

namespace TrialWorld.Contracts
{
    /// <summary>
    /// Extension methods for RectangleDto
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Creates a RectangleDto from top-left coordinates
        /// </summary>
        /// <param name="top">Top coordinate</param>
        /// <param name="left">Left coordinate</param>
        /// <param name="width">Width of rectangle</param>
        /// <param name="height">Height of rectangle</param>
        /// <param name="timestamp">Timestamp when rectangle was detected</param>
        /// <returns>A new RectangleDto instance</returns>
        public static RectangleDto FromTopLeft(float top, float left, float width, float height, TimeSpan timestamp)
        {
            return new RectangleDto(left, top, width, height, timestamp);
        }
        
        /// <summary>
        /// Gets the top coordinate of the rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <returns>The top coordinate</returns>
        public static float GetTop(this RectangleDto rectangle)
        {
            return rectangle.Y;
        }
        
        /// <summary>
        /// Gets the left coordinate of the rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <returns>The left coordinate</returns>
        public static float GetLeft(this RectangleDto rectangle)
        {
            return rectangle.X;
        }
        
        /// <summary>
        /// Gets the top coordinate of the rectangle as an integer
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <returns>The top coordinate as an integer</returns>
        public static int GetTopInt(this RectangleDto rectangle)
        {
            return (int)rectangle.Y;
        }
        
        /// <summary>
        /// Gets the left coordinate of the rectangle as an integer
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <returns>The left coordinate as an integer</returns>
        public static int GetLeftInt(this RectangleDto rectangle)
        {
            return (int)rectangle.X;
        }
    }
}
