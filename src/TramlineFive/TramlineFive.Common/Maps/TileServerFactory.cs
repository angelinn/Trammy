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

    public static TileLayer CreateTileLayer(IDataFetchStrategy dataFetchStrategy)
    {
        return new TileLayer(CreateTileSource("https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}.png"), dataFetchStrategy: dataFetchStrategy, renderFetchStrategy: new RenderFetchStrategy());
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
