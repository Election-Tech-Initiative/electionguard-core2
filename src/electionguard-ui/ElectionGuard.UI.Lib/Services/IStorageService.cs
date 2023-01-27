using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Services
{
    /// <summary>
    /// Interface to read/write file data 
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Write a string to a file on a drive
        /// </summary>
        /// <param name="path">folder where the file will be created</param>
        /// <param name="filename">name of the file</param>
        /// <param name="data">data to save into file</param>
        public void ToFile(string path, string filename, string data);

        /// <summary>
        /// Read in the contents of a file
        /// </summary>
        /// <param name="filename">filename including path</param>
        /// <returns>the string data from the file</returns>
        public string FromFile(string filename);
    }

    /// <summary>
    /// static service to get the configured storage service to use
    /// </summary>
    public static class StorageService
    {
        /// <summary>
        /// Creates an instance of a storage service to use
        /// </summary>
        /// <returns>a storage service</returns>
        public static IStorageService GetInstance()
        {
            // TODO: this will need to be enhanced when we add yubikey support
            return new DriveService();
        }
    }

}
