using System.Collections.Generic;
using TrialWorld.Presentation.Interfaces;
using System.Windows;


namespace TrialWorld.Presentation.Services
{
    public class ThemeService : IThemeService
    {
        private string _currentTheme = "Light";
        private readonly List<string> _availableThemes = new() { "Light", "Dark", "HighContrast" };
        
        public string CurrentTheme => _currentTheme;
        
        public IEnumerable<string> AvailableThemes => _availableThemes;
        
        public void SetTheme(string themeName)
        {
            if (_availableThemes.Contains(themeName))
            {
                _currentTheme = themeName;
                // TODO: Implement theme switching logic
            }
        }
    }
}
