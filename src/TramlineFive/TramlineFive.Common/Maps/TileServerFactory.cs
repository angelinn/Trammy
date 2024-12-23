using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling.Rendering;
using System;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Common.Maps;

public static class TileServerFactory
{
    private static readonly BruTile.Attribution OpenStreetMap = new BruTile.Attribution(
            "© OpenStreetMap", "http://www.openstreetmap.org/copyright");

    public static TileLayer CreateTileLayer(string tileServer, IDataFetchStrategy dataFetchStrategy, IRenderFetchStrategy renderFetchStrategy)
    {
        return new TileLayer(CreateTileSource(tileServer), dataFetchStrategy: dataFetchStrategy, renderFetchStrategy: renderFetchStrategy);
    }

    private static HttpTileSource CreateTileSource(string name)
    {
        return new HttpTileSource(new GlobalSphericalMercator(0, 19),
            name,
            //"https://maps.wikimedia.org/osm-intl/{z}/{x}/{y}.png",
            new[] { "a", "b", "c" }, name: name,
            attribution: OpenStreetMap);
    }
}
