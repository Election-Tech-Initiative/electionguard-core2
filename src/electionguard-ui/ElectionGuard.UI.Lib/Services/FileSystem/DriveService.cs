using System.IO.Compression;

namespace ElectionGuard.UI.Lib.Services;

public class DriveService : IStorageService
{
    private string _rootDirectoryPath;

    public DriveService(string rootDirectoryPath)
    {
        _rootDirectoryPath = rootDirectoryPath;
    }

    public DriveService() : this(Path.GetTempPath())
    {
    }

    public void ToFile(string fileName, string content)
    {
        var filePath = Path.Combine(_rootDirectoryPath, fileName);
        File.WriteAllText(filePath, content);
    }

    public string FromFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

        var filePath = Path.Combine(_rootDirectoryPath, fileName);
        
        if (!File.Exists(filePath)) throw new FileNotFoundException(nameof(filePath));

        return File.ReadAllText(filePath);
    }

    public void UpdatePath(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        if (! Directory.Exists(path))
        {
            // create directory
            Directory.CreateDirectory(path);

            _rootDirectoryPath = path;
        }
    }
}
