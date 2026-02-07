using System.IO.Compression;

namespace ClientBackupAgent.Services;

public class CompressionService
{
    public Task<string> CompressAsync(string bakPath, CancellationToken ct)
    {
        var zipPath = Path.ChangeExtension(bakPath, ".zip");

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(bakPath, Path.GetFileName(bakPath), CompressionLevel.Optimal);
        }

        return Task.FromResult(zipPath);
    }
}
