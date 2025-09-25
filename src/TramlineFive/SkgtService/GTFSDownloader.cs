using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkgtService;

public class GTFSDownloader
{
    private readonly string StaticDataUrl;
    private readonly string DestinationPath;
    private readonly string ExtractPath;

    public GTFSDownloader(string staticDataUrl, string destinationPath, string extractPath)
    {
        StaticDataUrl = staticDataUrl;
        DestinationPath = destinationPath;
        ExtractPath = extractPath;
    }

    public async Task DownloadStaticDataAsync()
    {
        if (File.Exists(DestinationPath))
        {
            Console.WriteLine("GTFS ZIP already exists, skipping download.");
            return;
        }

        using HttpClient client = new HttpClient();
        Console.WriteLine($"Downloading GTFS feed from {StaticDataUrl}...");

        byte[] data = await client.GetByteArrayAsync(StaticDataUrl);
        await File.WriteAllBytesAsync(DestinationPath, data);

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
            ZipFile.ExtractToDirectory(DestinationPath, ExtractPath, true);
            Console.WriteLine($"Extracted GTFS to {ExtractPath}");
        }
    }
}
