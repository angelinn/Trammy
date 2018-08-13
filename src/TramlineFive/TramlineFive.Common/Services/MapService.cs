using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using SkgtService;
using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TramlineFive.Common.Maps;
using TramlineFive.Common.Messages;

namespace TramlineFive.Common.Services
{
    public class MapService
    {
        private Map map;
        private SymbolStyle pinStyle;
        private SymbolStyle userStyle;
        private List<Feature> features;
        private IInteractionService interaction;

        private Queue<MapClickedResponseMessage> messages = new Queue<MapClickedResponseMessage>();

        private ManualResetEvent mapClickResetEvent = new ManualResetEvent(false);

        private const int STOP_THRESHOLD = 500;

        public MapService()
        {
            Messenger.Default.Register<MapClickedResponseMessage>(this, (m) =>
            {
                messages.Enqueue(m);
                mapClickResetEvent.Set();
            });
        }

        public void Initialize(Map map)
        {
            this.map = map;
            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            map.Info += OnMapInfo;
            LoadPinStyles();
            LoadUserLocationPin();

            ILayer stopsLayer = LoadStops();
            map.Layers.Add(stopsLayer);
            map.InfoLayers.Add(stopsLayer);

            interaction = SimpleIoc.Default.GetInstance<IInteractionService>();
        }

        public void MoveTo(Point point, int zoom = 14)
        {
            map.NavigateTo(point);
        }

        public void MoveToUser(Point point)
        {
            map.NavigateTo(point);
            map.NavigateTo(map.Resolutions[16]);

            if (map.Layers.Last().Name == "User location layer")
                map.Layers.Remove(map.Layers.Last());

            Feature feature = new Feature
            {
                Geometry = point,
                Styles = new List<IStyle>
                {
                    new SymbolStyle
                    {
                        Enabled = userStyle.Enabled,
                        BitmapId = userStyle.BitmapId
                    }
                }
            };

            Layer layer = new Layer
            {
                Name = "User location layer",
                Style = null,
                DataSource = new MemoryProvider(new List<Feature>() { feature })
            };

            map.Layers.Add(layer);
            map.ViewChanged(true);
        }

        private void LoadUserLocationPin()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.person.png");

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            userStyle = new SymbolStyle
            {
                BitmapId = bitmapId
            };
        }

        private void LoadPinStyles()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.pin.png");

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            pinStyle = new SymbolStyle
            {
                BitmapId = bitmapId,
                Enabled = false
            };
        }


        private ILayer LoadStops()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.stops-bg.json");

            features = new List<Feature>();

            List<StopLocation> stops = new StopsLoader().LoadStops(stream);
            foreach (var location in stops)
            {
                Point stopLocation = new Point(location.Lon, location.Lat);
                Point stopMapLocation = SphericalMercator.FromLonLat(stopLocation.X, stopLocation.Y);

                Feature feature = new Feature
                {
                    Geometry = stopMapLocation,
                    Styles = new List<IStyle>
                    {
                        new SymbolStyle
                        {
                            Enabled = pinStyle.Enabled,
                            BitmapId = pinStyle.BitmapId
                        }
                    }
                };

                feature["stopObject"] = location;
                features.Add(feature);
            }

            return new Layer
            {
                Name = "Stops layer",
                DataSource = new MemoryProvider(features),
                Style = null
            };
        }

        public void MoveToStop(string code)
        {
            foreach (Feature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;
                if (location.Code == code)
                {
                    Point point = new Point(location.Lon, location.Lat);
                    Point local = SphericalMercator.FromLonLat(point.X, point.Y);

                    SymbolStyle style = feature.Styles.First() as SymbolStyle;
                    style.Enabled = true;

                    MoveTo(local, 16);
                }
            }
        }

        private void OnMapInfo(object sender, Mapsui.UI.InfoEventArgs e)
        {
            Messenger.Default.Send(new MapClickedMessage());
            mapClickResetEvent.WaitOne();
            mapClickResetEvent.Reset();

            MapClickedResponseMessage message = messages.Dequeue();
            if (message.Handled)
                return;

            if (e.Feature != null && e.Feature.Styles.First().Enabled)
            {
                StopLocation location = e.Feature["stopObject"] as StopLocation;

                Messenger.Default.Send(new StopSelectedMessage(location.Code));
                return;
            }

            foreach (Feature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;

                Point point = new Point(location.Lon, location.Lat);
                Point local = SphericalMercator.FromLonLat(point.X, point.Y);
                Point difference = e.WorldPosition - local;

                if (Math.Abs(difference.X) < STOP_THRESHOLD && Math.Abs(difference.Y) < STOP_THRESHOLD)
                {
                    SymbolStyle style = feature.Styles.First() as SymbolStyle;
                    style.Enabled = true;
                }
            }
            
            map.ViewChanged(true);
        }

    }
}
