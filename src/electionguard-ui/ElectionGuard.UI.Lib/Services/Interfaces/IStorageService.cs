using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

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
    public void ToFile(string filename, string content);

    /// <summary>
    /// Write multiple files to a drive
    /// </summary>
    /// <param name="contents"></param>
    public void ToFiles(List<FileContents> files);

    /// <summary>
    /// Read in the contents of a file
    /// </summary>
    /// <param name="filename">filename including path</param>
    /// <returns>the string data from the file</returns>
    /// <exception cref="FileNotFoundException">File does not exist</exception>
    /// <exception cref="ArgumentNullException">filename not provided</exception>
    public string FromFile(string filename);

    /// <summary>
    /// Update the path for a file
    /// </summary>
    /// <param name="path">new path</param>
    /// <exception cref="ArgumentNullException">Path is null</exception>
    /// <exception cref="ArgumentException">Path does not exist</exception>
    public void UpdatePath(string path);
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
