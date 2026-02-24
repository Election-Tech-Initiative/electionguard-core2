using System.IO.Compression;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;
public class ZipStorageService : IStorageService
{
    private string _zipFile = string.Empty;

    private ZipArchiveMode ZipMode => File.Exists(_zipFile) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

    public ZipStorageService(string zipFile)
    {
        UpdatePath(zipFile);
    }

    public ZipStorageService() : this(Path.GetTempFileName()) { }

    public void ToFile(string fileName, string content)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        using var zipArchive = ZipFile.Open(_zipFile, ZipMode);

        var entry = zipArchive.CreateEntry(fileName);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream);

        writer.Write(content);
    }

    public void ToFiles(List<FileContents> fileContents)
    {
        using var zipArchive = ZipFile.Open(_zipFile, ZipMode);
        foreach (var fileContent in fileContents)
        {
            var entry = zipArchive.CreateEntry(fileContent.FileName);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);
            writer.Write(fileContent.Contents);
        }
    }

    public string FromFile(string fileName)
    {
        using var zipArchive = ZipFile.OpenRead(_zipFile);
        var entry = zipArchive.GetEntry(fileName) ?? throw new FileNotFoundException(nameof(fileName));
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void UpdatePath(string zipFile)
    {
        _zipFile = zipFile;
    }
}
