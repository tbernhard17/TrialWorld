using System;
using System.Collections.Generic;

namespace TrialWorld.Presentation.Interfaces
{
    /// <summary>
    /// Interface for theme services that manage application theming
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Sets the application theme
        /// </summary>
        /// <param name="themeName">The name of the theme to apply</param>
        void SetTheme(string themeName);
        
        /// <summary>
        /// Gets the current theme name
        /// </summary>
        string CurrentTheme { get; }
        
        /// <summary>
        /// Gets available theme names
        /// </summary>
        IEnumerable<string> AvailableThemes { get; }
    }
}
