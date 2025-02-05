using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Extensions.Cache;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling.Rendering;
using System;
using System.IO;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.Services.Maps;

public static class TileServerFactory
{
    private static readonly BruTile.Attribution OpenStreetMap = new BruTile.Attribution(
            "© OpenStreetMap", "http://www.openstreetmap.org/copyright");

    public static TileLayer CreateTileLayer(string tileServer, IDataFetchStrategy dataFetchStrategy, IRenderFetchStrategy renderFetchStrategy, string dbPath)
    {
        return new TileLayer(CreateTileSource(tileServer, dbPath), dataFetchStrategy: dataFetchStrategy, renderFetchStrategy: renderFetchStrategy);
    }

    private static HttpTileSource CreateTileSource(string name, string dbFolder)
    {

        return new HttpTileSource(new GlobalSphericalMercator(0, 19),
            name,
            //"https://maps.wikimedia.org/osm-intl/{z}/{x}/{y}.png",
            new[] { "a", "b", "c" }, name: name,
            attribution: OpenStreetMap, 
            persistentCache: new SqlitePersistentCache("tiles", folder: dbFolder));
    }
}
