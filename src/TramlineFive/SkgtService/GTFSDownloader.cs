using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkgtService;

public class GTFSDownloader
{
    private readonly string StaticDataUrl;
    private readonly string LocalDownloadPath;
    private readonly string ExtractPath;

    public GTFSDownloader(string staticDataUrl, string localDownloadPath, string extractPath)
    {
        StaticDataUrl = staticDataUrl;
        LocalDownloadPath = localDownloadPath;
        ExtractPath = extractPath;
    }

    public async Task DownloadStaticDataAsync()
    {
        if (File.Exists(LocalDownloadPath))
        {
            Console.WriteLine("GTFS ZIP already exists, skipping download.");
            return;
        }

        using HttpClient client = new HttpClient();
        Console.WriteLine($"Downloading GTFS feed from {StaticDataUrl}...");

        byte[] data = await client.GetByteArrayAsync(StaticDataUrl);
        await File.WriteAllBytesAsync(LocalDownloadPath, data);

        Console.WriteLine("Download complete.");
    }

    public void ExtractStaticData()
    {
        if (File.Exists(Path.Combine(ExtractPath, "stops.txt")))
        {
            Console.WriteLine("Data already extracted.");
        }
        else
        {
            ZipFile.ExtractToDirectory(LocalDownloadPath, ExtractPath, true);
            Console.WriteLine($"Extracted GTFS to {ExtractPath}");
        }
    }
}
