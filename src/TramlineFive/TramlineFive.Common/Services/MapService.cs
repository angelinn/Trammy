using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkgtService;
using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TramlineFive.Common.Maps;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using Mapsui.Projections;
using KdTree;
using GalaSoft.MvvmLight.Ioc;
using Mapsui.Animations;

namespace TramlineFive.Common.Services;

public class MapService
{
    private readonly MPoint CENTER_OF_SOFIA = new(23.3196994, 42.6969899);
    private const int ANIMATION_MS = 600;
    private Map map;
    private SymbolStyle pinStyle;
    private SymbolStyle userStyle;
    public static List<StopLocation> Stops;

    private KdTree<float, IFeature> stopsTree;
    private Dictionary<string, IFeature> stopsDictionary;
    private readonly List<Style> activeStyles = new List<Style>();
    private Navigator navigator;

    private readonly LocationService locationService;

    public int MaxPinsZoom { get; set; } = 15;
    public int MaxTextZoom { get; set; } = 17; 

    public MapService(LocationService locationService)
    {
        this.locationService = locationService;
    }
    
    public async Task Initialize(Map map, Navigator navigator, string tileServer)
    {
        this.map = map;
        this.navigator = navigator;

        await TileServerSettings.LoadTileServersAsync();

        await SetupMapAsync(tileServer);
    }

    public async Task SetupMapAsync(string tileServer)
    {
        MPoint point = SphericalMercator.FromLonLat(CENTER_OF_SOFIA);
        map.Home = n => { n.CenterOn(point); n.ZoomTo(map.Navigator.Resolutions[17]); ShowNearbyStops(point); };

        map.Layers.Add(TileServerFactory.CreateTileLayer(tileServer ?? "carto-light"));

        LoadPinStyles();

        ILayer stopsLayer = await LoadStops();
        map.Layers.Add(stopsLayer);

        navigator.ViewportChanged += (sender, args) => SendMapRefreshMessage();
    }

    public void MoveTo(MPoint position, int zoom, bool home = false)
    {
        MPoint point = SphericalMercator.FromLonLat(position);

        if (home)
            map.Home = n => n.CenterOn(point);

        navigator.CenterOnAndZoomTo(point, map.Navigator.Resolutions[zoom], ANIMATION_MS, Easing.CubicOut);
    }

    public void MoveToUser(Position position, bool home = false)
    {
        (double x, double y) = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

        MPoint userLocationMap = new MPoint(x, y);

        navigator.CenterOnAndZoomTo(userLocationMap, map.Navigator.Resolutions[17], ANIMATION_MS, home ? null : Easing.Linear);

        ShowNearbyStops(userLocationMap);
    }

    private void SendMapRefreshMessage()
    {
        Messenger.Default.Send(new RefreshMapMessage());
    }

    private SymbolStyle trolleyPinStyle;
    private SymbolStyle tramPinStyle;

    private void LoadPinStyles()
    {
        int bitmapId = typeof(MapService).LoadSvgId("MTS_Bus_icon.svg");

        pinStyle = new SymbolStyle
        {
            BitmapId = bitmapId,
            Enabled = false,
            SymbolScale = 0.14f
        };

        int trolleyId = typeof(MapService).LoadSvgId("MTS_TrolleyBus_icon.svg");

        trolleyPinStyle = new SymbolStyle
        {
            BitmapId = trolleyId,
            Enabled = false,
            SymbolScale = 0.14f
        };

        int tramId = typeof(MapService).LoadSvgId("MTS_Tram_icon.svg");

        tramPinStyle = new SymbolStyle
        {
            BitmapId = tramId,
            Enabled = false,
            SymbolScale = 0.4f
        };
    }

    private async Task<ILayer> LoadStops()
    {
        List<IFeature> features = new List<IFeature>();
        stopsTree = new KdTree<float, IFeature>(2, new KdTree.Math.GeoMath());
        stopsDictionary = new Dictionary<string, IFeature>();

        Stops = await StopsLoader.LoadStopsAsync();
        await StopsLoader.LoadRoutesAsync();

        foreach (var location in Stops)
        {
            var lines = StopsLoader.GetLinesForStop(location.Code);
            if (lines.Count == 0)
                continue;

            MPoint stopLocation = new MPoint(location.Lon, location.Lat);
            MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(stopLocation.X, stopLocation.Y));

            SymbolStyle symbolStyle = pinStyle;
            Offset offset = new Offset(0, -32);
            if (lines.Any(line => line.VehicleType ==  "trolley"))
                symbolStyle = trolleyPinStyle;
            else if (lines.Any(line => line.VehicleType == "tram"))
            { 
                symbolStyle = tramPinStyle;
                offset = new Offset(0, -40);
            }

            IFeature feature = new PointFeature(stopMapLocation)
            {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Enabled = symbolStyle.Enabled,
                        BitmapId = symbolStyle.BitmapId,
                        SymbolOffset = new Offset(0, 30),
                        SymbolScale = symbolStyle.SymbolScale,
                        MaxVisible = map.Navigator.Resolutions[MaxPinsZoom]
                    },
                    new LabelStyle
                    {
                        Enabled = symbolStyle.Enabled,
                        MaxVisible = map.Navigator.Resolutions[MaxTextZoom],
                        Text = $"{location.PublicName} ({location.Code})",
                        Offset = offset,
                        Font = new Font { Size = 11 }
                        
                        //Opacity = 0.7f,
                    }
                }
            };

            feature["stopObject"] = location;
            features.Add(feature);

            stopsTree.Add(new float[] { (float)location.Lat, (float)location.Lon }, feature);
            stopsDictionary[location.Code] = feature;
        }

        return new Layer
        {
            Name = "Stops layer",
            DataSource = new MemoryProvider(features),
            Style = null,
            IsMapInfoLayer = true
        };
    }
    
    public void MoveToStop(string code)
    {
        foreach (Style style in activeStyles)
            style.Enabled = false;

        activeStyles.Clear();

        IFeature feature = stopsDictionary[code];

        StopLocation location = feature["stopObject"] as StopLocation;
        MPoint point = new MPoint(location.Lon, location.Lat);

        foreach (Style style in feature.Styles)
        {
            activeStyles.Add(style);
            style.Enabled = true;
        }

        MoveTo(point, 17);
    }

    public async Task ShowNearbyStops(MPoint position, bool hideOthers = false)
    {
        await Task.Run(() =>
        {
            List<KeyValuePair<double, StopLocation>> nearbyStops = new List<KeyValuePair<double, StopLocation>>();

            MPoint pointLan = SphericalMercator.ToLonLat(position);
            var neighbours = stopsTree.GetNearestNeighbours(new float[] { (float)pointLan.Y, (float)pointLan.X }, 10).Where(n => n != null).Select(n => n.Value).ToList();

            foreach (Style style in activeStyles)
                style.Enabled = false;

            activeStyles.Clear();

            int i = 0;
            foreach (var neighbour in neighbours)
            {
                StopLocation location = neighbour["stopObject"] as StopLocation;
                nearbyStops.Add(new KeyValuePair<double, StopLocation>(i++, location));

                foreach (Style style in neighbour.Styles)
                {
                    activeStyles.Add(style);
                    style.Enabled = true;
                }
            }

            //FilterStops(neighbours);

            Messenger.Default.Send(new NearbyStopsMessage(nearbyStops.Select(p => p.Value).ToList()));
        });
    }

    private void FilterStops(List<IFeature> features)
    {
        bool[] processed = new bool[features.Count];

        int i = 0;
        foreach (IFeature feature in features)
        {
            if (!processed[i])
            {
                processed[i] = true;
                StopLocation location = feature["stopObject"] as StopLocation;

                foreach (Style style in feature.Styles)
                {
                    activeStyles.Add(style);

                    System.Diagnostics.Debug.WriteLine($"Enabling all styles for {location.Code}");
                    style.Enabled = true;
                }

                int j = 0;
                foreach (IFeature otherFeature in features)
                {
                    StopLocation otherLocation = otherFeature["stopObject"] as StopLocation;
                    double distance = locationService.GetDistance(location.Lat, location.Lon, otherLocation.Lat, otherLocation.Lon);

                    if (distance < 10 && !processed[j])
                    {
                        processed[j] = true;

                        foreach (Style style in otherFeature.Styles)
                        {
                            if (style is LabelStyle)
                            {
                                activeStyles.Remove(style);
                                style.Enabled = false;
                                System.Diagnostics.Debug.WriteLine($"Disabling label style for {location.Code}");
                            }
                            else
                            {
                                activeStyles.Add(style);
                                style.Enabled = true;
                                System.Diagnostics.Debug.WriteLine($"Enabling symbol style for {location.Code}");
                            }
                        }
                    }

                    ++j;
                }
            }

            ++i;
        }
    }

    public void OnMapInfo(MapInfoEventArgs e)
    {
        Messenger.Default.Send(new MapClickedMessage());

        if (e.MapInfo.Feature != null && e.MapInfo.Feature.Styles.First().Enabled)
        {
            StopLocation location = e.MapInfo.Feature["stopObject"] as StopLocation;

            Messenger.Default.Send(new StopSelectedMessage(location.Code, true));
            return;
        }
        //ShowNearbyStops(e.MapInfo.WorldPosition);
        //SendMapRefreshMessage();
    }

}
