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
using TramlineFive.Common.Maps;
using TramlineFive.Common.Messages;

namespace TramlineFive.Common.Services
{
    public class MapService
    {
        private static Map map;
        private static SymbolStyle pinStyle;
        private static SymbolStyle userStyle;
        private static List<Feature> features;
        private static IInteractionService interaction;

        private const int STOP_THRESHOLD = 500;

        public static void Initialize(Map map)
        {
            MapService.map = map;
            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            map.Info += OnMapInfo;
            LoadPinStyles();
            LoadUserLocationPin();

            ILayer stopsLayer = LoadStops();
            map.Layers.Add(stopsLayer);
            map.InfoLayers.Add(stopsLayer);

            interaction = SimpleIoc.Default.GetInstance<IInteractionService>();
        }

        public static void MoveTo(Point point)
        {
            map.NavigateTo(point);
            map.NavigateTo(map.Resolutions[14]);
        }

        public static void MoveToUser(Point point)
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

        private static void LoadUserLocationPin()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.person.png");

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            userStyle = new SymbolStyle
            {
                BitmapId = bitmapId
            };
        }

        private static void LoadPinStyles()
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


        private static ILayer LoadStops()
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

        private static void OnMapInfo(object sender, Mapsui.UI.InfoEventArgs e)
        {
            if (e.Feature != null && e.Feature.Styles.First().Enabled)
            {
                StopLocation location = e.Feature["stopObject"] as StopLocation;
                interaction.ChangeTab(interaction.VirtualTablesIndex);

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
