using System.IO.Compression;
using Amazon.Util.Internal;

namespace ElectionGuard.UI.Lib.Services;
public class ZipStorageService : IStorageService
{
    private string _zipFile;

    public ZipStorageService(string zipFile)
    {
        UpdatePath(zipFile);
    }

    public ZipStorageService() : this(Path.GetTempFileName()) { }

    public void ToFile(string fileName, string content)
    {
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

        using var zipArchive = ZipFile.Open(_zipFile, ZipArchiveMode.Update);
        var entry = zipArchive.CreateEntry(fileName);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }

    public string FromFile(string fileName)
    {
        using var zipArchive = ZipFile.OpenRead(_zipFile);
        var entry = zipArchive.GetEntry(fileName);

        if (entry == null) throw new FileNotFoundException(nameof(fileName));

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void UpdatePath(string zipFile)
    {
        if (!File.Exists(zipFile))
        {
            ZipFile.Open(zipFile, ZipArchiveMode.Create).Dispose();
        }
        _zipFile = zipFile;
    }
}
