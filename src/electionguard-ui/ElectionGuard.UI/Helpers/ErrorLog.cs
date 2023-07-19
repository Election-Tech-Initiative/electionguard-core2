using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace ElectionGuard.UI.Helpers
{
    static class ErrorLog
    {
        private static string ElectionGuardPath = "electionguard";
        private static string LogPath = "logs";

        public static string CreateLogPath()
        {
            var folder = Path.Combine(SpecialDirectories.MyDocuments, ElectionGuardPath, LogPath);
            _ = Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
