using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace ElectionGuard.UI.Helpers
{
    static class ErrorLog
    {
        private static string ElectionGuardPath = "electionguard";
        private static string LogPath = "logs";
        private static string LogCrashedFile = "crashed.txt";

        public static string CreateLogPath()
        {
            var folder = Path.Combine(FileSystem.Current.AppDataDirectory, ElectionGuardPath, LogPath);
            _ = Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Create an empty file when the app crashes.
        /// </summary>
        public static void CreateCrashedFile()
        {
            var folder = CreateLogPath();
            File.AppendAllText(Path.Combine(folder, LogCrashedFile), " ");
        }

        /// <summary>
        /// Remove the file used to determine a crash
        /// </summary>
        public static void DeleteCrashedFile()
        {
            var folder = CreateLogPath();
            File.Delete(Path.Combine(folder, LogCrashedFile));
        }

        /// <summary>
        /// Indicator if the application crashed the last time it was run
        /// </summary>
        /// <returns>Last run caused the app to crash</returns>
        public static bool AppPreviousCrashed()
        {
            var folder = CreateLogPath();
            return File.Exists(Path.Combine(folder, LogCrashedFile));
        }
    }
}
