using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
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
        private static List<Feature> features;

        public static void Initialize(Map map)
        {
            MapService.map = map;
            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            map.Info += NativeMap_Info;
            LoadPinStyles();

            ILayer stopsLayer = LoadStops();
            map.Layers.Add(stopsLayer);
            map.InfoLayers.Add(stopsLayer);
        }

        private static void LoadPinStyles()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.pin.png");
            foreach (string name in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine(name);

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            var centerOfSofia = new Mapsui.Geometries.Point(23.3219, 42.6977);
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.X, centerOfSofia.Y);

            pinStyle = new SymbolStyle
            {
                BitmapId = bitmapId,
                Enabled = false
            };
        }


        private static ILayer LoadStops()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.stops.json");

            features = new List<Feature>();

            List<StopLocation> stops = new StopsLoader().LoadStops(stream);
            foreach (var location in stops)
            {
                var centerOfSofia = new Mapsui.Geometries.Point(location.Lon, location.Lat);
                var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfSofia.X, centerOfSofia.Y);

                Feature feature = new Feature
                {
                    Geometry = sphericalMercatorCoordinate,
                    Styles = new List<IStyle> { new SymbolStyle
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

        private static void NativeMap_Info(object sender, Mapsui.UI.InfoEventArgs e)
        {
            if (e.Feature != null && e.Feature.Styles.First().Enabled)
            {
                StopLocation location = e.Feature["stopObject"] as StopLocation;
                SimpleIoc.Default.GetInstance<IInteractionService>().ChangeTab(1);

                Messenger.Default.Send(new StopSelectedMessage(location.Code));
                return;
            }

            foreach (var l in features)
            {
                StopLocation location = l["stopObject"] as StopLocation;

                var point = new Mapsui.Geometries.Point(location.Lon, location.Lat);
                var local = SphericalMercator.FromLonLat(point.X, point.Y);
                var difference = e.WorldPosition - local;

                if (Math.Abs(difference.X) < 500 && Math.Abs(difference.Y) < 500)
                {
                    SymbolStyle style = l.Styles.First() as SymbolStyle;
                    style.Enabled = true;
                }
            }
            (sender as Mapsui.Map).ViewChanged(true);
        }

    }
}
