using System.Reflection;
using TrialWorld.Presentation.Interfaces;

namespace TrialWorld.Presentation.Services
{
    public class VersionService : IVersionService
    {
        public string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null
                ? $"{version.Major}.{version.Minor}.{version.Build}"
                : "1.0.0";
        }
    }
}