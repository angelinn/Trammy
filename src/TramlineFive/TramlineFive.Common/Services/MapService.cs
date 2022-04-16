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

namespace TramlineFive.Common.Services
{
    public class MapService
    {
        private Map map;
        private SymbolStyle pinStyle;
        private SymbolStyle userStyle;
        private List<IFeature> features;
        public static List<StopLocation> Stops;

        private INavigator navigator;

        private const int STOP_THRESHOLD = 500;

        public int MaxPinsZoom { get; set; } = 6;
        public int MaxTextZoom { get; set; } = 3;

        public MapService()
        {

        }

        public async Task Initialize(Map map, INavigator navigator)
        {
            this.map = map;
            MPoint centerOfSofia = new MPoint(23.3219, 42.6977);
            MPoint point = SphericalMercator.FromLonLat(centerOfSofia);
            map.Home = n => { n.CenterOn(point); n.ZoomTo(map.Resolutions[15]); ShowNearbyStops(point); };

            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            LoadPinStyles();

            this.navigator = navigator;

            ILayer stopsLayer = await LoadStops();
            map.Layers.Add(stopsLayer);

            navigator.Navigated = (sender, args) => SendMapRefreshMessage();
        }

        public async Task ReloadMapAsync()
        {
            MPoint centerOfSofia = new MPoint(23.3219, 42.6977);
            MPoint point = SphericalMercator.FromLonLat(centerOfSofia);
            map.Home = n => { n.CenterOn(point); n.ZoomTo(map.Resolutions[15]); };

            map.Layers.Clear();
            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            LoadPinStyles();

            ILayer stopsLayer = await LoadStops();
            map.Layers.Add(stopsLayer);

            navigator.Navigated = (sender, args) => SendMapRefreshMessage();
        }

        public void MoveTo(MPoint position, int zoom, bool home = false)
        {
            MPoint point = SphericalMercator.FromLonLat(position);

            if (home)
                map.Home = n => n.CenterOn(point);

            navigator.NavigateTo(point, map.Resolutions[zoom], 1000, Easing.CubicOut);
        }

        private void SendMapRefreshMessage()
        {
            Messenger.Default.Send(new RefreshMapMessage());
        }

        public void MoveToUser(Position position, bool home = false)
        {
            (double x, double y) = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

            MPoint userLocationMap = new MPoint(x, y);

            navigator.NavigateTo(userLocationMap, map.Resolutions[16], 1000, home ? null : Easing.Linear);

            ShowNearbyStops(userLocationMap);
        }

        private void LoadPinStyles()
        {
            int bitmapId = typeof(MapService).LoadSvgId("pin.svg");

            pinStyle = new SymbolStyle
            {
                BitmapId = bitmapId,
                Enabled = false,
                SymbolScale = 0.3f
            };
        }


        private async Task<ILayer> LoadStops()
        {
            features = new List<IFeature>();

            Stops = await StopsLoader.LoadStopsAsync();
            foreach (var location in Stops)
            {
                MPoint stopLocation = new MPoint(location.Lon, location.Lat);
                MPoint stopMapLocation = SphericalMercator.FromLonLat(new MPoint(stopLocation.X, stopLocation.Y));

                IFeature feature = new PointFeature(stopMapLocation)
                {
                    Styles = new List<IStyle>
                    {
                        new SymbolStyle
                        {
                            Enabled = pinStyle.Enabled,
                            BitmapId = pinStyle.BitmapId,
                            SymbolOffset = new Offset(0, 30),
                            SymbolScale = pinStyle.SymbolScale,
                            MaxVisible = MaxPinsZoom + 0.5
                        },
                        new LabelStyle
                        {
                            Enabled = pinStyle.Enabled,
                            MaxVisible = MaxTextZoom + 0.5,
                            Text = $"{location.PublicName} ({location.Code})",
                            Offset = new Offset(0, -45)
                            //Opacity = 0.7f,
                        }
                    }
                }; 

                feature["stopObject"] = location;
                features.Add(feature);
            }

            return new Layer
            {
                Name = "Stops layer",
                DataSource = new MemoryProvider<IFeature>(features),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public void MoveToStop(string code)
        {
            foreach (IFeature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;
                if (location.Code == code)
                {
                    MPoint point = new MPoint(location.Lon, location.Lat);

                    foreach (Style style in feature.Styles)
                        style.Enabled = true;

                    MoveTo(point, 16);
                }
            }
        }

        public void ShowNearbyStops(MPoint position, bool hideOthers = false)
        {
            List<KeyValuePair<double, StopLocation>> nearbyStops = new List<KeyValuePair<double, StopLocation>>();

            foreach (IFeature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;

                MPoint point = new MPoint(location.Lon, location.Lat);
                MPoint local = SphericalMercator.FromLonLat(point);
                MPoint difference = position - local;

                if (Math.Abs(difference.X) < STOP_THRESHOLD && Math.Abs(difference.Y) < STOP_THRESHOLD)
                {
                    nearbyStops.Add(new KeyValuePair<double, StopLocation>(Math.Abs(difference.X) + Math.Abs(difference.Y), location));

                    foreach (Style style in feature.Styles)
                        style.Enabled = true;
                }
                else 
                    foreach(Style style in feature.Styles)
                        style.Enabled = false;
            }

            Messenger.Default.Send(new NearbyStopsMessage(nearbyStops.OrderBy(p => p.Key).Select(p => p.Value).ToList()));
        } 

        public void OnMapInfo(object sender, MapInfoEventArgs e)
        {
            Messenger.Default.Send(new MapClickedMessage());
 
            if (e.MapInfo.Feature != null && e.MapInfo.Feature.Styles.First().Enabled)
            {
                StopLocation location = e.MapInfo.Feature["stopObject"] as StopLocation;

                Messenger.Default.Send(new StopSelectedMessage(location.Code, true));
                return;
            }
            ShowNearbyStops(e.MapInfo.WorldPosition);
            SendMapRefreshMessage();
        }

    }
}
