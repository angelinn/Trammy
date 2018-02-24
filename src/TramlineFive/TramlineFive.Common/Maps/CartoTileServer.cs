using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;
using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.Maps
{
    public static class HumanitarianTileServer
    {
        private static readonly BruTile.Attribution OpenStreetMap = new BruTile.Attribution(
                "© OpenStreetMap contributors", "http://www.openstreetmap.org/copyright");

        public static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "Carto" };
        }

        private static HttpTileSource CreateTileSource()
        {
            return new HttpTileSource(new GlobalSphericalMercator(0, 18),
                "http://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png",
                new[] { "a", "b", "c" }, name: "Humanitarian",
                attribution: OpenStreetMap);
        }
    }
}
