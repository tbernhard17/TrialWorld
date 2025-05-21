namespace TrialWorld.Core.Models
{
    /// <summary>
    /// Defines the type of comparison to perform between media segments
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// Compare the original media without any enhancements
        /// </summary>
        Original,

        /// <summary>
        /// Compare with enhanced audio only
        /// </summary>
        AudioEnhanced,

        /// <summary>
        /// Compare with enhanced video only
        /// </summary>
        VideoEnhanced,

        /// <summary>
        /// Compare with both audio and video enhancements
        /// </summary>
        FullyEnhanced,

        /// <summary>
        /// Compare with custom enhancement settings
        /// </summary>
        Custom
    }
}