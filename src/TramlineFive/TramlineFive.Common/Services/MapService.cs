using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkgtService;
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
using Mapsui.Animations;
using SkgtService.Models.Json;
using SkgtService.Models;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui.Tiling.Fetcher;
using Mapsui.Extensions;

namespace TramlineFive.Common.Services;

public class MapService
{
    private readonly MPoint CENTER_OF_SOFIA = new(23.3196994, 42.6969899);
    private const int ANIMATION_MS = 600;
    private Map map;
    private SymbolStyle pinStyle;
    private SymbolStyle userStyle;
    public static List<StopInformation> Stops;

    private KdTree<float, IFeature> stopsTree;
    private Dictionary<string, IFeature> stopsDictionary;
    private readonly List<Style> activeStyles = new List<Style>();

    private readonly LocationService locationService;
    private readonly PublicTransport publicTransport;

    public int MaxPinsZoom { get; set; } = 15;
    public int MaxTextZoom { get; set; } = 17;

    public MapService(LocationService locationService, PublicTransport publicTransport)
    {
        this.locationService = locationService;
        this.publicTransport = publicTransport;
    }

    public async Task LoadInitialMap(Map map)
    {        
        await TileServerSettings.LoadTileServersAsync();

        map.Layers.Add(TileServerFactory.CreateTileLayer("carto-light", new DataFetchStrategy()));
    }

    public async Task Initialize(Map map, string tileServer, string fetchStrategy)
    {
        this.map = map;

        await SetupMapAsync(tileServer, fetchStrategy);
    }

    public async Task SetupMapAsync(string tileServer, string fetchingStrategy)
    {
        MPoint point = SphericalMercator.FromLonLat(CENTER_OF_SOFIA);
        map.Home = n =>
        {
            n.CenterOn(point);
            n.ZoomTo(map.Navigator.Resolutions[17]);
            _ = ShowNearbyStops(point);

            WeakReferenceMessenger.Default.Send(new MapLoadedMessage());
        };

        IDataFetchStrategy fetchStrategy = fetchingStrategy switch
        {
            "None" => null,
            "Minimal" => new MinimalDataFetchStrategy(),
            "Full" => new DataFetchStrategy(),
            _ => new DataFetchStrategy()
        };

        //map.Layers.Add(TileServerFactory.CreateTileLayer(tileServer ?? "carto-light", fetchStrategy));

        //await Task.Delay(100);

        LoadPinStyles();
        ILayer stopsLayer = await LoadStops();
        map.Layers.Add(stopsLayer);

        MyLocationLayer locationLayer = new MyLocationLayer(map);
        locationLayer.Enabled = true;
        map.Layers.Add(locationLayer);

        if (!WeakReferenceMessenger.Default.IsRegistered<UpdateLocationMessage>(this))
        {
            WeakReferenceMessenger.Default.Register<UpdateLocationMessage>(this, (r, m) =>
            {
                var point = SphericalMercator.FromLonLat(m.Position.Longitude, m.Position.Latitude).ToMPoint();
                locationLayer.UpdateMyLocation(point);
            });
        }

        map.Navigator.ViewportChanged += (sender, args) => SendMapRefreshMessage();
    }

    public void MoveTo(MPoint position, int zoom, bool home = false)
    {
        MPoint point = SphericalMercator.FromLonLat(position);

        if (home)
            map.Home = n => n.CenterOn(point);

        map.Navigator.CenterOnAndZoomTo(point, map.Navigator.Resolutions[zoom], ANIMATION_MS, Easing.CubicOut);
    }

    public void MoveToUser(Position position, bool home = false)
    {
        (double x, double y) = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

        MPoint userLocationMap = new MPoint(x, y);

        map.Navigator.CenterOnAndZoomTo(userLocationMap, map.Navigator.Resolutions[17], ANIMATION_MS, home ? null : Easing.Linear);

        _ = ShowNearbyStops(userLocationMap);
    }

    private void SendMapRefreshMessage()
    {
        WeakReferenceMessenger.Default.Send(new RefreshMapMessage());
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

        await publicTransport.LoadData();

        Stops = publicTransport.Stops;

        foreach (var location in Stops)
        {
            MPoint stopLocation = new MPoint(location.Lon, location.Lat);
            MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(stopLocation.X, stopLocation.Y));

            SymbolStyle symbolStyle = pinStyle;
            Offset offset = new Offset(0, -32);
            if (location.Lines.Any(line => line.VehicleType == TransportType.Trolley))
                symbolStyle = trolleyPinStyle;
            else if (location.Lines.Any(line => line.VehicleType == TransportType.Tram))
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
                        Font = new Font { Size = 11 },
                        BackColor = new Brush(new Color(255, 255, 255, 200)),
                        
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

    public void MoveAroundStop(string code)
    {
        if (!stopsDictionary.TryGetValue(code, out IFeature feature))
            return;
        StopInformation location = feature["stopObject"] as StopInformation;
        MPoint point = new MPoint(location.Lon, location.Lat);
        MoveTo(point, 17);

        MPoint localPoint = SphericalMercator.FromLonLat(point);

        ShowNearbyStops(localPoint);
    }

    public void MoveToStop(string code)
    {
        foreach (Style style in activeStyles)
            style.Enabled = false;

        activeStyles.Clear();

        if (!stopsDictionary.TryGetValue(code, out IFeature feature))
            return;

        StopInformation location = feature["stopObject"] as StopInformation;
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
            List<KeyValuePair<double, StopInformation>> nearbyStops = new();

            MPoint pointLan = SphericalMercator.ToLonLat(position);
            var neighbours = stopsTree.GetNearestNeighbours(new float[] { (float)pointLan.Y, (float)pointLan.X }, 10).Where(n => n != null).Select(n => n.Value).ToList();

            foreach (Style style in activeStyles)
                style.Enabled = false;

            activeStyles.Clear();

            int i = 0;
            foreach (var neighbour in neighbours)
            {
                StopInformation location = neighbour["stopObject"] as StopInformation;
                nearbyStops.Add(new KeyValuePair<double, StopInformation>(i++, location));

                foreach (Style style in neighbour.Styles)
                {
                    activeStyles.Add(style);
                    style.Enabled = true;
                }
            }

            //FilterStops(neighbours);

            if (nearbyStops.Count > 0)
                WeakReferenceMessenger.Default.Send(new NearbyStopsMessage(nearbyStops.Select(p => p.Value).ToList()));
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
                StopInformation location = feature["stopObject"] as StopInformation;

                foreach (Style style in feature.Styles)
                {
                    activeStyles.Add(style);

                    System.Diagnostics.Debug.WriteLine($"Enabling all styles for {location.Code}");
                    style.Enabled = true;
                }

                int j = 0;
                foreach (IFeature otherFeature in features)
                {
                    StopInformation otherLocation = otherFeature["stopObject"] as StopInformation;
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
        WeakReferenceMessenger.Default.Send(new MapClickedMessage());

        if (e.MapInfo.Feature != null && e.MapInfo.Feature.Styles.First().Enabled)
        {
            StopInformation location = e.MapInfo.Feature["stopObject"] as StopInformation;

            WeakReferenceMessenger.Default.Send(new StopSelectedMessage(new StopSelectedMessagePayload(location.Code, true)));
            return;
        }
        //ShowNearbyStops(e.MapInfo.WorldPosition);
        //SendMapRefreshMessage();
    }

}
