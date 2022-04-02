using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkgtService;
using SkgtService.Models.Locations;
<<<<<<< HEAD
using SkiaSharp;
using Svg.Skia;
=======
>>>>>>> c7d59d5 (Revert "Attempt to load svg pin.")
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

namespace TramlineFive.Common.Services
{
    public class MapService
    {
        private Map map;
        private SymbolStyle pinStyle;
        private SymbolStyle userStyle;
        private List<Feature> features;

        private INavigator navigator;

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

        public async Task Initialize(Map map, INavigator navigator)
        {
            this.map = map;
            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            LoadPinStyles();
            LoadUserLocationPin();

            this.navigator = navigator;

            ILayer stopsLayer = await LoadStops();
            map.Layers.Add(stopsLayer);
        }

        public void MoveTo(Position position, int? zoom = null, bool home = false)
        {
            Point point = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

            if (home)
                map.Home = n => n.CenterOn(point);

            navigator.CenterOn(point);
            if (zoom.HasValue)
                navigator.ZoomTo(map.Resolutions[zoom.Value]);

            SendMapRefreshMessage();
        }

        private void SendMapRefreshMessage()
        {
            Messenger.Default.Send(new RefreshMapMessage());
        }

        public void MoveToUser(Position position, bool home = false)
        {
            //navigator.CenterOn(point);

            //if (map.Layers.Last().Name == "User location layer")
            //    map.Layers.Remove(map.Layers.Last());

            //Feature feature = new Feature
            //{
            //    Geometry = point,
            //    Styles = new List<IStyle>
            //    {
            //        new SymbolStyle
            //        {
            //            Enabled = userStyle.Enabled,
            //            BitmapId = userStyle.BitmapId
            //        }
            //    }
            //};

            //Layer layer = new Layer
            //{
            //    Name = "User location layer",
            //    Style = null,
            //    DataSource = new MemoryProvider(new List<Feature>() { feature })
            //};

            //map.Layers.Add(layer);

            Messenger.Default.Send(new UpdateLocationMessage(position));
            Point userLocationMap = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

            if (home)
                map.Home = n => n.CenterOn(userLocationMap);

            navigator.CenterOn(userLocationMap);
            navigator.ZoomTo(map.Resolutions[16]);

            ShowNearbyStops(userLocationMap);
            SendMapRefreshMessage();
        }

<<<<<<< HEAD
        private int CreateBitmap(Stream data, double scale)
        {
            var svg = new SKSvg();
            svg.Load(data);

            var info = new SKImageInfo(
                (int)(svg.Picture.CullRect.Width * scale),
                (int)(svg.Picture.CullRect.Height * scale))
            {
                AlphaType = SKAlphaType.Premul
            };

            var bitmap = new SKBitmap(info);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear();
            canvas.Scale((float)scale);

            // paint.ColorFilter = SKColorFilter.CreateBlendMode(
            //     Color.ToSKColor(),
            //     SKBlendMode.SrcIn
            // ); // use the source color

            var paint = new SKPaint() { IsAntialias = true };
            canvas.DrawPicture(svg.Picture, paint);

            var image = SKImage.FromBitmap(bitmap);
            var bitmapData = image.Encode(SKEncodedImageFormat.Png, 100);
            var bitmapDataArray = bitmapData.ToArray();
            var stream = new MemoryStream(bitmapDataArray);
            var bitmapId = BitmapRegistry.Instance.Register(stream);
            return bitmapId;
        }

        private void LoadUserLocationPin()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.location.svg");

            var svg = new SKSvg();
            svg.Load(stream); 

            int bitmapId = BitmapRegistry.Instance.Register(svg.Picture);
=======
        private void LoadUserLocationPin()
        {
            Assembly assembly = typeof(MapService).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.person.png");

            var bitmapId = BitmapRegistry.Instance.Register(stream);
>>>>>>> c7d59d5 (Revert "Attempt to load svg pin.")

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


        private async Task<ILayer> LoadStops()
        {
            features = new List<Feature>();

            List<StopLocation> stops = await StopsLoader.LoadStopsAsync();
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
                            BitmapId = pinStyle.BitmapId,
                            SymbolOffset = new Offset(0, 10)
                        },
                        new LabelStyle
                        {
                            Enabled = pinStyle.Enabled,
                            Text = $"{location.PublicName} ({location.Code})",
                            Offset = new Offset(0, -50),
                            //Opacity = 0.7f
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
                Style = null,
                IsMapInfoLayer = true
            };
        }

        public void MoveToStop(string code)
        {
            foreach (Feature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;
                if (location.Code == code)
                {
                    Position point = new Position(location.Lat, location.Lon);

                    foreach (Style style in feature.Styles)
                        style.Enabled = true;
                    
                    MoveTo(point, 16);
                }
            }
        }

        private void ShowNearbyStops(Point position)
        {
            foreach (Feature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;

                Point point = new Point(location.Lon, location.Lat);
                Point local = SphericalMercator.FromLonLat(point.X, point.Y);
                Point difference = position - local;

                if (Math.Abs(difference.X) < STOP_THRESHOLD && Math.Abs(difference.Y) < STOP_THRESHOLD)
                {
                    foreach (Style style in feature.Styles)
                        style.Enabled = true;
                }
            }
        }

        public void OnMapInfo(object sender, MapInfoEventArgs e)
        {
            Messenger.Default.Send(new MapClickedMessage());
            mapClickResetEvent.WaitOne();
            mapClickResetEvent.Reset();

            MapClickedResponseMessage message = messages.Dequeue();
            if (message.Handled)
                return;

            if (e.MapInfo.Feature != null && e.MapInfo.Feature.Styles.First().Enabled)
            {
                StopLocation location = e.MapInfo.Feature["stopObject"] as StopLocation;

                Messenger.Default.Send(new StopSelectedMessage(location.Code, false));
                return;
            }
            ShowNearbyStops(e.MapInfo.WorldPosition);
            SendMapRefreshMessage();
        }

    }
}
