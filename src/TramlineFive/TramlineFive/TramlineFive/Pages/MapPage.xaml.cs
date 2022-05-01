using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.UI.Forms;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using System;
using System.Threading.Tasks;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    public partial class MapPage : Grid
    {
        private bool initialized;
        private bool isOpened;

        private readonly MapService mapService;

        public MapPage()
        {
            InitializeComponent();

            Messenger.Default.Register<ShowMapMessage>(this, (m) => ToggleMap(m));
            Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
            Messenger.Default.Register<UpdateLocationMessage>(this, m => map.MyLocationLayer.UpdateMyLocation(new Position(m.Position.Latitude, m.Position.Longitude)));

            Map nativeMap = new Map
            {
                BackColor = Mapsui.Styles.Color.White,
                CRS = "EPSG:3857"
            };

            mapService = ServiceContainer.ServiceProvider.GetService<MapService>();
            Task _ = (BindingContext as MapViewModel).Initialize(nativeMap, map.Navigator);

            map.Info += OnMapInfo;

            map.Map = nativeMap;
            map.TouchAction += OnMapTouchAction;
        }

        private void OnMapTouchAction(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {
            if (e.ActionType == SkiaSharp.Views.Forms.SKTouchAction.Released)
            {
                mapService.ShowNearbyStops(new MPoint(map.Viewport.CenterX, map.Viewport.CenterY), true);
                map.Refresh();

                System.Diagnostics.Debug.WriteLine($"Show stops");
            }

            System.Diagnostics.Debug.WriteLine($"Touch: {e.ActionType} {map.Viewport.CenterX} {map.Viewport.CenterY}");
        }

        private void OnMapInfo(object sender, Mapsui.UI.MapInfoEventArgs e)
        {
            (BindingContext as MapViewModel).OnMapInfo(e);
        }

        private void ShowVirtualTables(int linesCount)
        {
            int coef = linesCount > 2 ? 2 : linesCount;
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;

            Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height - slideMenu.HeightRequest);

            Task _ = slideMenu.TranslateTo(0, 0, 400);
            animation.Commit(map, "ShowMap", 60, 400);
        }

        private void HideVirtualTables()
        {
            Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height);

            Task _ = slideMenu.TranslateTo(0, Height, 400);
            animation.Commit(map, "Expand", 60, 400);
        }

        private void ToggleMap(ShowMapMessage message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                isOpened = message.Show;

                if (!message.Show)
                    HideVirtualTables();
                else if (message.Show)
                    ShowVirtualTables(message.ArrivalsCount);
            });
        }

        public void OnAppearing()
        {
            if (initialized)
                return;

            initialized = true;
            Task task = (BindingContext as MapViewModel).LoadAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            slideMenu.TranslationY = isOpened ? 0 : Height;
            map.HeightRequest = Height;
        }   
    }
}
