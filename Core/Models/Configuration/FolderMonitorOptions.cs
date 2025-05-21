using System.Collections.Generic;

namespace TrialWorld.Core.Models.Configuration
{
    public class FolderMonitorOptions
    {
        /// <summary>
        /// List of absolute paths to folders that should be monitored for new media files
        /// </summary>
        public List<string> MonitoredFolders { get; set; } = new List<string>();

        /// <summary>
        /// Whether subfolders within monitored folders should also be monitored
        /// </summary>
        public bool IncludeSubfolders { get; set; } = true;

        /// <summary>
        /// Whether to automatically process existing files found during startup
        /// </summary>
        public bool ProcessExistingFiles { get; set; } = true;
    }
}
