using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.Common.Services.Maps;

public static class TileServerSettings
{
    private const string TILE_SERVERS_FILE = "TramlineFive.Common.tileservers.json";

    public static Dictionary<string, string> TileServers { get; private set; }

    public static async Task LoadTileServersAsync()
    {
        using Stream tileServersFile = typeof(TileServerSettings).Assembly.GetManifestResourceStream(TILE_SERVERS_FILE);
        using StreamReader reader = new StreamReader(tileServersFile);

        string json = await reader.ReadToEndAsync();

        TileServers = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
}
