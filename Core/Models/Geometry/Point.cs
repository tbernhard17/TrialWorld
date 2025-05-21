using System;

namespace TrialWorld.Core.Models.Geometry
{
    /// <summary>
    /// Represents a 2D point with normalized coordinates (0.0-1.0)
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Gets or sets the X coordinate (normalized 0.0-1.0)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate (normalized 0.0-1.0)
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Calculates the Euclidean distance to another point
        /// </summary>
        public double DistanceTo(Point other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Calculates the angle in degrees to another point
        /// </summary>
        public double AngleTo(Point other)
        {
            var dx = other.X - X;
            var dy = other.Y - Y;
            return Math.Atan2(dy, dx) * 180 / Math.PI;
        }

        /// <summary>
        /// Determines if this point is within a given rectangle
        /// </summary>
        public bool IsInside(Rectangle rect)
        {
            return X >= rect.X &&
                   X <= rect.X + rect.Width &&
                   Y >= rect.Y &&
                   Y <= rect.Y + rect.Height;
        }

        /// <summary>
        /// Creates a new point by interpolating between this point and another
        /// </summary>
        public Point Interpolate(Point other, double t)
        {
            return new Point
            {
                X = X + (other.X - X) * t,
                Y = Y + (other.Y - Y) * t
            };
        }
    }
}