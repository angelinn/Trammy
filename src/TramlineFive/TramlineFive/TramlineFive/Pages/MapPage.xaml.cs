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

            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap(m));
            Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
            Messenger.Default.Register<UpdateLocationMessage>(this, m => map.MyLocationLayer.UpdateMyLocation(new Position(m.Position.Latitude, m.Position.Longitude)));

            Map nativeMap = new Map
            {
                BackColor = Mapsui.Styles.Color.White,
                CRS = "EPSG:3857"
            };

            mapService = ServiceContainer.ServiceProvider.GetService<MapService>();
            Task _ = (BindingContext as MapViewModel).Initialize(nativeMap, map.Navigator);

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

        private async Task ShowVirtualTables(int linesCount)
        {
            int coef = linesCount > 2 ? 2 : linesCount;
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;

            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height - slideMenu.HeightRequest + 30);

            await slideMenu.TranslateTo(0, 0, 400);
            map.HeightRequest = Height - slideMenu.HeightRequest + 30;
            //animation.Commit(map, "ShowMap", 256, 400);
        }

        private async Task HideVirtualTables()
        {
            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height);

            map.HeightRequest = Height;
            await slideMenu.TranslateTo(0, Height, 400);
            //animation.Commit(map, "Expand", 16, 400);
        }

        private async Task ToggleMap(ShowMapMessage message)
        {
            if (message.ElapsedMilliseconds > 0)
            {
                long difference = 400 - message.ElapsedMilliseconds;
                if (difference > 0)
                    await Task.Delay((int)difference);
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                isOpened = message.Show;

                if (!message.Show)
                    await HideVirtualTables();
                else if (message.Show)
                    await ShowVirtualTables(message.ArrivalsCount);
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
