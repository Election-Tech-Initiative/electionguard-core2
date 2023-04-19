using System.IO.Compression;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Service to allow simple creation and managing of zip files
/// </summary>
public class ZipService
{
    /// <summary>
    /// Creates a zip file if it does not exist
    /// </summary>
    /// <param name="zipFileName">file name and path where to make the zip file</param>
    private void CreateZip(string zipFileName)
    {
        if (!File.Exists(zipFileName))
        {
            // Create the empty zip file
            ZipFile.CreateFromDirectory(".", zipFileName, CompressionLevel.Optimal, false);
        }
    }

    /// <summary>
    /// Adds a file to the given zip file.  If the zip file does not exist, it creates it first
    /// </summary>
    /// <param name="zipFileName">filename and path of the zip file to use / make</param>
    /// <param name="fileList">list of file data to add to the zip file</param>
    public void AddFilesToZip(string zipFileName, List<FileContents> fileList)
    {
        CreateZip(zipFileName);

        using var archive = ZipFile.Open(zipFileName, ZipArchiveMode.Update);

        foreach (var file in fileList)
        {
            // Create a new entry for the file in the zip file
            var entry = archive.CreateEntry(file.FileName.Replace('\\', '/'), CompressionLevel.Optimal);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(file.Contents);
        }
    }
}
