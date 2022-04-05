using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using SkgtService;
using SkgtService.Models.Locations; 
using SkiaSharp; 
using Svg.Skia;
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
using Mapsui.Utilities;
using Mapsui.Tiling;

namespace TramlineFive.Common.Services
{
    public class MapService
    {
        private Map map;
        private SymbolStyle pinStyle;
        private SymbolStyle userStyle;
        private List<IFeature> features;

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
            MPoint centerOfSofia = new MPoint(23.3219, 42.6977); 
            MPoint point = SphericalMercator.FromLonLat(centerOfSofia);
            map.Home = n => { n.CenterOn(point); n.ZoomTo(map.Resolutions[14]); };

            map.Layers.Add(HumanitarianTileServer.CreateTileLayer());
            LoadPinStyles();
            //LoadUserLocationPin();

            this.navigator = navigator;

            ILayer stopsLayer = await LoadStops();
            map.Layers.Add(stopsLayer);

            navigator.Navigated = (sender, args) =>
            {
                SendMapRefreshMessage();
            };
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
            (double x, double y) = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

            MPoint userLocationMap = new MPoint(x, y);

            navigator.NavigateTo(userLocationMap, map.Resolutions[16], 1000, home ? null : Easing.Linear);

            //navigator.CenterOn(userLocationMap);
            //navigator.ZoomTo(map.Resolutions[16]);

            ShowNearbyStops(userLocationMap);
        } 

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
            Stream stream = assembly.GetManifestResourceStream("TramlineFive.Common.person.png");

            var bitmapId = BitmapRegistry.Instance.Register(stream);

            userStyle = new SymbolStyle
            {
                BitmapId = bitmapId
            };
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

            List<StopLocation> stops = await StopsLoader.LoadStopsAsync();
            foreach (var location in stops)
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
                            SymbolScale = pinStyle.SymbolScale
                        },
                        new LabelStyle
                        {
                            Enabled = pinStyle.Enabled,
                            Text = $"{location.PublicName} ({location.Code})",
                            Offset = new Offset(0, -45),
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

        private void ShowNearbyStops(MPoint position)
        {
            foreach (IFeature feature in features)
            {
                StopLocation location = feature["stopObject"] as StopLocation;

                MPoint point = new MPoint(location.Lon, location.Lat);
                MPoint local = SphericalMercator.FromLonLat(point);
                MPoint difference = position - local;

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
