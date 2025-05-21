using System;

namespace TrialWorld.Core.Models.Geometry
{
    /// <summary>
    /// Represents a normalized rectangle with coordinates and dimensions (all values 0.0-1.0)
    /// </summary>
    public class Rectangle
    {
        /// <summary>
        /// Gets or sets the X coordinate of top-left corner (normalized 0.0-1.0)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of top-left corner (normalized 0.0-1.0)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the rectangle (normalized 0.0-1.0)
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the rectangle (normalized 0.0-1.0)
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets the area of the rectangle
        /// </summary>
        public double Area => Width * Height;

        /// <summary>
        /// Gets the center point of the rectangle
        /// </summary>
        public Point Center => new Point { X = X + Width / 2, Y = Y + Height / 2 };

        /// <summary>
        /// Determines if this rectangle contains another rectangle
        /// </summary>
        public bool Contains(Rectangle other)
        {
            return X <= other.X &&
                   Y <= other.Y &&
                   X + Width >= other.X + other.Width &&
                   Y + Height >= other.Y + other.Height;
        }

        /// <summary>
        /// Calculates intersection over union with another rectangle
        /// </summary>
        public double IntersectionOverUnion(Rectangle other)
        {
            var x1 = Math.Max(X, other.X);
            var y1 = Math.Max(Y, other.Y);
            var x2 = Math.Min(X + Width, other.X + other.Width);
            var y2 = Math.Min(Y + Height, other.Y + other.Height);

            if (x2 < x1 || y2 < y1) return 0;

            var intersection = (x2 - x1) * (y2 - y1);
            var union = Area + other.Area - intersection;

            return intersection / union;
        }
    }
}