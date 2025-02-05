using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using SkgtService;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using Mapsui.Projections;
using KdTree;
using Mapsui.Animations;
using SkgtService.Models;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui.Tiling.Fetcher;
using Mapsui.Extensions;
using Mapsui.Tiling.Rendering;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Manipulations;

namespace TramlineFive.Common.Services.Maps;

public class MapService
{
    private readonly MPoint CENTER_OF_SOFIA = new(23.3196994, 42.6969899);
    private const int ANIMATION_MS = 600;
    private const string ROUTE_LAYER = "Route Layer";
    private const string STOPS_LAYER = "Stops layer";

    private Map map;
    private SymbolStyle busPinStyle;
    private SymbolStyle trolleyPinStyle;
    private SymbolStyle tramPinStyle;
    private SymbolStyle subwayStyle;
    private SymbolStyle nightStyle;
    private readonly List<Style> activeStyles = new List<Style>();

    private MyLocationLayer locationLayer;

    private KdTree<float, IFeature> stopsTree;
    private Dictionary<string, IFeature> stopsDictionary;
    private readonly ManualResetEvent stopsFinishedLoadingEvent = new(false);

    private IDataFetchStrategy dataStrategy;
    private IRenderFetchStrategy renderStrategy;
    private string savedTileServer;

    private readonly LocationService locationService;
    private readonly PublicTransport publicTransport;

    private Stack<(MPoint, int)> navigationStack = new();

    private bool isShowingRoute;

    public double OverlayHeightInPixels { get; set; }

    public int MaxPinsZoom { get; set; } = 15;
    public int MaxTextZoom { get; set; } = 17;


    public MapService(LocationService locationService, PublicTransport publicTransport)
    {
        this.locationService = locationService;
        this.publicTransport = publicTransport;

        WeakReferenceMessenger.Default.Register<RefreshStopsMessage>(this, async (r, m) => await OnStopsRefreshed());
        WeakReferenceMessenger.Default.Register<ShowRouteMessage>(this, (r, m) => ShowRoute(m));
    }

    private void ShowRoute(ShowRouteMessage showRouteMessage)
    {
        IEnumerable<string> wkts = showRouteMessage.Direction.Segments.Select(s => s.Wkt);

        WKTReader reader = new WKTReader();
        List<GeometryFeature> features = new List<GeometryFeature>();

        foreach (string wkt in wkts)
        {
            Geometry geometry = reader.Read(wkt);
            Coordinate[] newcoords = (geometry as LineString).Coordinates.
                Select(c => SphericalMercator.FromLonLat(c.Y, c.X)).
                Select(c => new Coordinate(c.x, c.y)).
                ToArray();

            geometry = new LineString(newcoords);

            GeometryFeature feature = new GeometryFeature { Geometry = geometry };
            feature.Styles.Add(new VectorStyle
            {
                Line = new Pen
                {
                    Color = Color.FromString(TransportConvertеr.TypeToColor(showRouteMessage.TransportType)),
                    Width = 5
                }
            });

            features.Add(feature);
        }

        Layer wktLayer = new Layer
        {
            Name = ROUTE_LAYER,
            DataSource = new MemoryProvider(features),
            Style = null
        };

        int stopsLayerIndex = map.Layers.ToList().IndexOf(map.Layers.FindLayer(STOPS_LAYER).First());
        map.Layers.Insert(stopsLayerIndex, wktLayer);

        List<string> lineStops = showRouteMessage.Direction.Segments.Select(s => s.StartStop)
            .Concat(showRouteMessage.Direction.Segments.Select(s => s.EndStop))
            .Distinct()
            .ToList();

        activeStyles.ForEach(s => s.Enabled = false);
        activeStyles.Clear();

        foreach (string stop in lineStops)
        {
            string numberStop = new string(stop.Where(c => char.IsDigit(c)).ToArray());
            if (!stopsDictionary.TryGetValue(numberStop, out IFeature feature))
                continue;

            foreach (IStyle style in feature.Styles)
            {
                activeStyles.Add(style as Style);
                style.Enabled = true;
                style.MaxVisible = style is SymbolStyle ? double.MaxValue : map.Navigator.Resolutions[MaxTextZoom - 3];
            }
        }

        isShowingRoute = true;
        map.Navigator.ZoomTo(map.Navigator.Resolutions[15], ANIMATION_MS, Easing.CubicOut);
    }

    public void HideRoutes()
    {
        if (isShowingRoute)
        {
            isShowingRoute = false;
            map.Layers.Remove((l) => l.Name == ROUTE_LAYER);
        }
    }

    public void LoadInitialMap(Map map, string tileServer, string dataFetchStrategy, string renderFetchStrategy)
    {
        this.map = map;
        this.map.Navigator.RotationLock = true;

        ChangeTileServer(tileServer, dataFetchStrategy, renderFetchStrategy);

        MPoint point = SphericalMercator.FromLonLat(CENTER_OF_SOFIA);
        map.Navigator.CenterOnAndZoomTo(point, map.Navigator.Resolutions[17]);
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
        map.Layers.Remove(map.Layers.Get(1));
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

        locationLayer = new MyLocationLayer(map);
        locationLayer.Enabled = true;
        map.Layers.Add(locationLayer);

        if (!WeakReferenceMessenger.Default.IsRegistered<UpdateLocationMessage>(this))
        {
            WeakReferenceMessenger.Default.Register<UpdateLocationMessage>(this, (r, m) =>
            {
                var point = SphericalMercator.FromLonLat(m.Position.Longitude, m.Position.Latitude).ToMPoint();
                locationLayer.UpdateMyLocation(point);

                map.Navigator.CenterOnAndZoomTo(point, map.Navigator.Resolutions[17], ANIMATION_MS, Easing.Linear);

                _ = ShowNearbyStops(point);
            });
        }

        await publicTransport.ReloadDataIfNeededAsync();
    }

    public void MoveToCurrentLocation()
    {
        map.Navigator.CenterOnAndZoomTo(locationLayer.MyLocation, map.Navigator.Resolutions[17], ANIMATION_MS, Easing.CubicOut);
    }

    public void MoveTo(MPoint position, int zoom, bool home = false, bool ignoreOverlayHeight = false)
    {
        HideRoutes();

        MPoint point = SphericalMercator.FromLonLat(position);

        if (home)
            map.Navigator.CenterOn(point);

        if (!ignoreOverlayHeight)
        {
            ScreenPosition worldPoint = map.Navigator.Viewport.WorldToScreen(point);
            worldPoint.Y += OverlayHeightInPixels / 2;

            point.Y = map.Navigator.Viewport.ScreenToWorld(worldPoint).Y;
            // Center only on the visible part of the screen
            //double overlayHeightInWorld = map.Navigator.Viewport.ScreenToWorld(new MPoint(0, 0)).Y -
            //    map.Navigator.Viewport.ScreenToWorld(new MPoint(0, OverlayHeightInPixels / 2)).Y;

            //point.Y -= overlayHeightInWorld;
        }

        map.Navigator.CenterOnAndZoomTo(point, map.Navigator.Resolutions[zoom], ANIMATION_MS, Easing.CubicOut);
    }

    private void LoadPinStyles()
    {
        busPinStyle = new SymbolStyle { ImageSource = "embedded://TramlineFive.Common.MTS_Bus_icon.svg", Enabled = false, SymbolScale = 0.14f };

        trolleyPinStyle = new SymbolStyle { ImageSource = "embedded://TramlineFive.Common.MTS_TrolleyBus_icon.svg", Enabled = false, SymbolScale = 0.14f };

        tramPinStyle = new SymbolStyle { ImageSource = "embedded://TramlineFive.Common.MTS_Tram_icon.svg", Enabled = false, SymbolScale = 0.4f };

        subwayStyle = new SymbolStyle { ImageSource = "embedded://TramlineFive.Common.subway_icon.svg", Enabled = false, SymbolScale = 0.4f };

        nightStyle = new SymbolStyle { ImageSource = "embedded://TramlineFive.Common.MTS_Bus_icon_night.svg", Enabled = false, SymbolScale = 0.4f };
    }

    private ILayer BuildStopsLayer()
    {
        List<IFeature> features = new List<IFeature>();
        stopsTree = new KdTree<float, IFeature>(2, new KdTree.Math.GeoMath());
        stopsDictionary = new Dictionary<string, IFeature>();

        foreach (var location in publicTransport.Stops)
        {
            MPoint stopLocation = new MPoint(location.Lon, location.Lat);
            MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(stopLocation.X, stopLocation.Y));

            var (symbolStyle, offset) = location.Type switch
            {
                TransportType.Trolley => (trolleyPinStyle, new Offset(0, -32)),
                TransportType.Tram => (tramPinStyle, new Offset(0, -40)),
                TransportType.Subway => (subwayStyle, new Offset(0, -40)),
                TransportType.NightBus => (nightStyle, new Offset(0, -32)),
                TransportType.Bus => (busPinStyle, new Offset(0, -32)),
                _ => (busPinStyle, new Offset(0, -32))
            };

            IFeature feature = new PointFeature(stopMapLocation)
            {
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Enabled = symbolStyle.Enabled,
                        ImageSource = symbolStyle.ImageSource,
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
            Style = null
        };
    }

    public void MoveAroundStop(string code, bool ignoreOverlayHeight)
    {
        if (!stopsDictionary.TryGetValue(code, out IFeature feature))
            return;
        StopInformation location = feature["stopObject"] as StopInformation;
        MPoint point = new MPoint(location.Lon, location.Lat);
        MoveTo(point, 17, false, ignoreOverlayHeight);

        MPoint localPoint = SphericalMercator.FromLonLat(point);

        _ = ShowNearbyStops(localPoint);
    }

    public void MoveToStop(string code)
    {
        activeStyles.ForEach(s => s.Enabled = false);
        activeStyles.Clear();

        if (!stopsDictionary.TryGetValue(code, out IFeature feature))
            return;

        StopInformation location = feature["stopObject"] as StopInformation;
        MPoint point = new MPoint(location.Lon, location.Lat);

        foreach (IStyle style in feature.Styles)
        {
            activeStyles.Add(style as Style);
            style.Enabled = true;
        }

        map.Navigator.RotateTo(0);
        map.Navigator.ZoomTo(map.Navigator.Resolutions[17]);
        MoveTo(point, 17);

        navigationStack.Push((point, 17));
    }

    public async Task ShowNearbyStops()
    {
        await ShowNearbyStops(new MPoint(map.Navigator.Viewport.CenterX, map.Navigator.Viewport.CenterY));
    }

    public async Task ShowNearbyStops(MPoint position)
    {
        if (isShowingRoute)
            return;

        await Task.Run(() =>
        {
            stopsFinishedLoadingEvent.WaitOne();

            List<StopInformation> nearbyStops = new List<StopInformation>();

            MPoint pointLan = SphericalMercator.ToLonLat(position);
            var neighbours = stopsTree.GetNearestNeighbours(new float[] { (float)pointLan.Y, (float)pointLan.X }, 10).Where(n => n != null).Select(n => n.Value).ToList();

            activeStyles.ForEach(s => s.Enabled = false);
            activeStyles.Clear();

            foreach (var neighbour in neighbours)
            {
                StopInformation location = neighbour["stopObject"] as StopInformation;
                nearbyStops.Add(location);

                foreach (IStyle style in neighbour.Styles)
                {
                    activeStyles.Add(style as Style);
                    style.Enabled = true;
                    style.MaxVisible = map.Navigator.Resolutions[style is SymbolStyle ? MaxPinsZoom : MaxTextZoom];
                }
            }
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
        MapInfo info = e.GetMapInfo(map.Layers.FindLayer(STOPS_LAYER));
        if (info.Feature != null && info.Feature.Styles.FirstOrDefault().Enabled)
        {
            StopInformation location = info.Feature["stopObject"] as StopInformation;

            WeakReferenceMessenger.Default.Send(new StopSelectedMessage(location.Code));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new HideVirtualTablesMessage());
            _ = ShowNearbyStops();
        }
    }

    public bool HandleGoBack()
    {
        if (isShowingRoute)
            HideRoutes();

        if (navigationStack.Count > 0)
        {
            (MPoint point, int zoom) = navigationStack.Pop();

            MPoint transformed = SphericalMercator.FromLonLat(point);
            map.Navigator.CenterOnAndZoomTo(transformed, map.Navigator.Resolutions[zoom], ANIMATION_MS, Easing.CubicOut);

            _ = ShowNearbyStops(transformed);

            return true;
        }

        return false;
    }
}
