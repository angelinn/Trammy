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
using Mapsui.Tiling.Rendering;
using RTools_NTS.Util;
using System.Diagnostics;

namespace TramlineFive.Common.Services.Maps;

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

    private ManualResetEvent stopsFinishedLoadingEvent = new(false);

    private IDataFetchStrategy dataStrategy;
    private IRenderFetchStrategy renderStrategy;
    private string savedTileServer;

    private readonly LocationService locationService;
    private readonly PublicTransport publicTransport;

    public double VisibleMapHeightInPixels { get; set; }

    public int MaxPinsZoom { get; set; } = 15;
    public int MaxTextZoom { get; set; } = 17;

    public MapService(LocationService locationService, PublicTransport publicTransport)
    {
        this.locationService = locationService;
        this.publicTransport = publicTransport;

        WeakReferenceMessenger.Default.Register<RefreshStopsMessage>(this, async (r, m) => await OnStopsRefreshed());
    }

    public void LoadInitialMap(Map map, string tileServer, string dataFetchStrategy, string renderFetchStrategy)
    {
        this.map = map;
        ChangeTileServer(tileServer, dataFetchStrategy, renderFetchStrategy);

        MPoint point = SphericalMercator.FromLonLat(CENTER_OF_SOFIA);
        map.Home = n =>
        {
            n.CenterOn(point);
            n.ZoomTo(map.Navigator.Resolutions[17]);
        };
    }

    public async Task Initialize(Map map)
    {
        this.map = map;

        await SetupMapAsync();
    }

    public void ChangeTileServer(string tileServer, string dataFetchStrategy, string renderFetchStrategy)
    {
        if (dataFetchStrategy != null)
        {
            dataStrategy = dataFetchStrategy switch
            {
                "None" => null,
                "MinimalDataFetchStrategy" => new MinimalDataFetchStrategy(),
                "DataFetchStrategy" => new DataFetchStrategy(),
                _ => new DataFetchStrategy()
            };
        }

        if (renderFetchStrategy != null)
        {
            renderStrategy = renderFetchStrategy switch
            {
                "None" => null,
                "MinimalRenderFetchStrategy" => new MinimalRenderFetchStrategy(),
                "RenderFetchStrategy" => new RenderFetchStrategy(),
                "TilingRenderFetchStrategy" => new TilingRenderFetchStrategy(null),
                _ => new RenderFetchStrategy()
            };
        }

        if (map.Layers.Count > 1)
            map.Layers.Remove(map.Layers.First());

        if (!string.IsNullOrEmpty(tileServer))
            savedTileServer = tileServer;

        map.Layers.Insert(0, TileServerFactory.CreateTileLayer(savedTileServer, dataStrategy, renderStrategy));
    }

    private async Task OnStopsRefreshed()
    {
        map.Layers.Remove(map.Layers[1]);
        map.Layers.Insert(1, BuildStopsLayer());

        await ShowNearbyStops(new MPoint(map.Navigator.Viewport.CenterX, map.Navigator.Viewport.CenterY));
    }

    public async Task SetupMapAsync()
    {
        MPoint point = SphericalMercator.FromLonLat(CENTER_OF_SOFIA);

        LoadPinStyles();

        publicTransport.StopsReadyEvent.WaitOne();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        map.Layers.Add(BuildStopsLayer());
        stopwatch.Stop();
        Debug.WriteLine($"Built stops layer in {stopwatch.ElapsedMilliseconds} ms");

        stopsFinishedLoadingEvent.Set();

        WeakReferenceMessenger.Default.Send(new MapLoadedMessage());

        stopwatch.Restart();
        await ShowNearbyStops(point);
        stopwatch.Stop();
        Debug.WriteLine($"Showed nearby stops in {stopwatch.ElapsedMilliseconds} ms");

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

        //map.Navigator.ViewportChanged += (sender, args) => SendMapRefreshMessage();
    }

    public void MoveTo(MPoint position, int zoom, bool home = false)
    {
        MPoint point = SphericalMercator.FromLonLat(position);

        if (home)
            map.Home = n => n.CenterOn(point);

        // Center only on the visible part of the screen
        double overlayHeightInWorld = map.Navigator.Viewport.ScreenToWorld(new MPoint(0, VisibleMapHeightInPixels)).Y -
            map.Navigator.Viewport.ScreenToWorld(new MPoint(0, 0)).Y;

        point.Y += overlayHeightInWorld;

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
    private SymbolStyle subwayStyle;
    private SymbolStyle nightStyle;

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


        int subwayId = typeof(MapService).LoadSvgId("subway_icon.svg");

        subwayStyle = new SymbolStyle
        {
            BitmapId = subwayId,
            Enabled = false,
            SymbolScale = 0.4f
        };

        int nightId = typeof(MapService).LoadSvgId("MTS_Bus_icon_night.svg");

        nightStyle = new SymbolStyle
        {
            BitmapId = nightId,
            Enabled = false,
            SymbolScale = 0.4f
        };
    }

    private ILayer BuildStopsLayer()
    {
        List<IFeature> features = new List<IFeature>();
        stopsTree = new KdTree<float, IFeature>(2, new KdTree.Math.GeoMath());
        stopsDictionary = new Dictionary<string, IFeature>();

        Stops = publicTransport.Stops;

        foreach (var location in Stops)
        {
            MPoint stopLocation = new MPoint(location.Lon, location.Lat);
            MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(stopLocation.X, stopLocation.Y));

            SymbolStyle symbolStyle = null;
            Offset offset = null;

            switch (location.Type)
            {
                case TransportType.Trolley:
                    symbolStyle = trolleyPinStyle;
                    offset = new Offset(0, -32);
                    break;
                case TransportType.Tram:
                    symbolStyle = tramPinStyle;
                    offset = new Offset(0, -40);
                    break;
                case TransportType.Subway:
                    symbolStyle = subwayStyle;
                    offset = new Offset(0, -40);
                    break;
                case TransportType.NightBus:
                    symbolStyle = nightStyle;
                    offset = new Offset(0, -32);
                    break;
                case TransportType.Bus:
                default:
                    symbolStyle = pinStyle;
                    offset = new Offset(0, -32);
                    break;
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
            stopsFinishedLoadingEvent.WaitOne();

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

                    Debug.WriteLine($"Enabling all styles for {location.Code}");
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
                                Debug.WriteLine($"Disabling label style for {location.Code}");
                            }
                            else
                            {
                                activeStyles.Add(style);
                                style.Enabled = true;
                                Debug.WriteLine($"Enabling symbol style for {location.Code}");
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
