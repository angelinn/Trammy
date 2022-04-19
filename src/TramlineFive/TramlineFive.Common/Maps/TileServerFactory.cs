using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using System;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.Maps;

public static class TileServerFactory
{
    private static readonly BruTile.Attribution OpenStreetMap = new BruTile.Attribution(
            "© OpenStreetMap contributors", "http://www.openstreetmap.org/copyright");

    public static TileLayer CreateTileLayer(string name)
    {
        return new TileLayer(CreateTileSource(name), dataFetchStrategy: new DataFetchStrategy()) { Name = name };
    }

    private static HttpTileSource CreateTileSource(string name)
    {
        return new HttpTileSource(new GlobalSphericalMercator(0, 18),
            TileServerSettings.TileServers[name],
            //"https://maps.wikimedia.org/osm-intl/{z}/{x}/{y}.png",
            new[] { "a", "b", "c" }, name: name,
            attribution: OpenStreetMap);
    }
}
