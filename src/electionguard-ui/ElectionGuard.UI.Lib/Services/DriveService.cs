using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElectionGuard.UI.Lib.Services
{
    /// <summary>
    /// Class to read and write file to the hard drive
    /// </summary>
    public class DriveService : IStorageService
    {
        /// <summary>
        /// Read in the contents of a file
        /// </summary>
        /// <param name="filename">filename including path</param>
        /// <returns>the string data from the file</returns>
        public string FromFile(string filename)
        {
            return File.ReadAllText(filename);
        }

        /// <summary>
        /// Write a string to a file on a drive
        /// </summary>
        /// <param name="path">folder where the file will be created</param>
        /// <param name="filename">name of the file</param>
        /// <param name="data">data to save into file</param>
        public void ToFile(string path, string filename, string data)
        {
            Directory.CreateDirectory(path);
            var name = Path.Combine(path, filename);
            File.WriteAllText(name, data);
        }
    }
}
