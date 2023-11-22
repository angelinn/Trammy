using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Maps;
using SkgtService;
using SkgtService.Models;
using KdTree;
using Mapsui.Providers;
using Mapsui.Styles;
using static System.Formats.Asn1.AsnWriter;
using SkgtService.Models.Locations;
using Mapsui.Utilities;
using ExCSS;
using Mapsui.Animations;

namespace TramlineFive.Common.Services;

public class LineMapService
{
    private SymbolStyle pinStyle;
    private SymbolStyle startStyle;
    private SymbolStyle finishStyle;

    private Dictionary<string, IFeature> stops = new();
    public Map Map { get; set; }
    private MRect routeBox;

    public async Task SetupMapAsync(string line, string type, Way route, string tileServer = null)
    {
        Map.Layers.Add(TileServerFactory.CreateTileLayer(tileServer ?? "carto-light"));
        Map.Home = (h) => ZoomToBox(h, routeBox);
        LoadPinStyles();

        ILayer stopsLayer = LoadStops(line, type, route);
        Map.Layers.Add(stopsLayer);
    }

    private void LoadPinStyles()
    {
        int bitmapId = typeof(MapService).LoadSvgId("MTS_Bus_icon.svg");

        pinStyle = new SymbolStyle
        {
            BitmapId = bitmapId,
            Enabled = false,
            SymbolScale = 0.1f
        };

        int startId = typeof(MapService).LoadSvgId("MTS_Bus_icon_start.svg");

        startStyle = new SymbolStyle
        {
            BitmapId = startId,
            Enabled = false,
            SymbolScale = 0.1f
        };

        int finishId = typeof(MapService).LoadSvgId("MTS_Bus_icon_finish.svg");

        finishStyle = new SymbolStyle
        {
            BitmapId = finishId,
            Enabled = false,
            SymbolScale = 0.1f
        };
    }

    private ILayer LoadStops(string line, string type, Way route)
    {
        List<IFeature> features = new List<IFeature>();

        double minLat = double.MaxValue;
        double minLon = double.MaxValue;
        double maxLon = double.MinValue;
        double maxLat = double.MinValue;

        for (int i = 0; i < route.Codes.Count; ++i)
        {
            StopLocation location = StopsLoader.StopsHash[route.Codes[i]];

            MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(location.Lon, location.Lat));

            if (stopMapLocation.Y > maxLon)
                maxLon = stopMapLocation.Y;
            if (stopMapLocation.Y < minLon)
                minLon = stopMapLocation.Y;

            if (stopMapLocation.X > maxLat)
                maxLat = stopMapLocation.X;
            if (stopMapLocation.X < minLat)
                minLat = stopMapLocation.X;

            int styleId = pinStyle.BitmapId;
            if (i == 0)
                styleId = startStyle.BitmapId;
            else if (i == route.Codes.Count - 1)
                styleId = finishStyle.BitmapId;

            IFeature feature = new PointFeature(stopMapLocation)
            {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        BitmapId = styleId,
                        //SymbolOffset = new Offset(0, 30),
                        SymbolScale = 0.1f,
                        //MaxVisible = map.Navigator.Resolutions[10]
                    }
                }
            };

            features.Add(feature);
            stops[location.Code] = feature;
        }

        routeBox = new MRect(minLat, minLon, maxLat, maxLon);
        ZoomToBox(Map.Navigator, routeBox);
        //BottomLeft = new MPoint(minLon, minLat),
        //BottomRight = new MPoint(minLon, maxLat),
        //TopLeft = new MPoint(maxLon, minLat),
        //TopRight = new MPoint(maxLon, maxLat

        return new Layer
        {
            Name = "Stops layer",
            DataSource = new MemoryProvider(features),
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private void ZoomToBox(Navigator navigator, MRect box)
    {
        navigator.ZoomToBox(box.Grow(box.Width * 0.1, box.Height * 0.1));
    }

    public void ZoomTo(string code)
    {
        if (stops.TryGetValue(code, out IFeature value))
        {
            PointFeature feature = value as PointFeature;
            Map.Navigator.CenterOnAndZoomTo(feature.Point, Map.Navigator.Resolutions[16], 600, Easing.Linear);
        }
    }

    public void ResetView()
    {
        ZoomToBox(Map.Navigator, routeBox);
    }
}
