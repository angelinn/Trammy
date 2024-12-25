using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using Microsoft.Maui.Layouts;
using SkgtService.Models.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Maui;
using Map = Mapsui.Map;

namespace TramlineFive.Pages
{
    public partial class MapPage : ContentPage
    {
        private bool initialized;
        private bool isOpened;

        private MapService mapService;

        public MapPage()
        {
            InitializeComponent();

            mapService = ServiceContainer.ServiceProvider.GetService<MapService>();
            mapService.LoadInitialMap(map.Map, 
                Preferences.Get(Settings.SelectedTileServerUrl, Defaults.TileServerUrl),
                Preferences.Get(Settings.FetchingStrategy, Defaults.DataFetchStrategy),
                Preferences.Get(Settings.RenderStrategy, Defaults.RenderFetchStrategy));

            map.UpdateInterval = 8;

            WeakReferenceMessenger.Default.Register<ShowMapMessage>(this, async (r, m) => await ToggleMap(m));
            
            //Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
        }

        protected override void OnAppearing()
        {
           
            base.OnAppearing();

            if (initialized)
                return;

            initialized = true;

            if (VersionTracking.IsFirstLaunchEver)
                Navigation.PushAsync(new LocationPromptPage());

            _ = Task.Run(async () =>
            {
                await LoadMapAsync();
                await Task.Delay(500);

                _ = (BindingContext as MapViewModel).LocalizeAsync(true);

                await Dispatcher.DispatchAsync(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    LazySearchBar.LoadViewAsync();
                    LazyVirtualTablesView.LoadViewAsync();
                    LazyFab.LoadViewAsync();
                    LazySuggestions.LoadViewAsync();
                    sw.Stop();
                    Debug.WriteLine($"Lazy load time: {sw.ElapsedMilliseconds}");
                });
            });
        }

        private async void OnMapTouchAction(object sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
        {
            if (e.ActionType == SkiaSharp.Views.Maui.SKTouchAction.Released)
            {
                await mapService.ShowNearbyStops(new MPoint(map.Map.Navigator.Viewport.CenterX, map.Map.Navigator.Viewport.CenterY), true);
                //map.Refresh();

                System.Diagnostics.Debug.WriteLine($"Show stops");
            }

            System.Diagnostics.Debug.WriteLine($"Touch: {e.ActionType} {map.Map.Navigator.Viewport.CenterX} {map.Map.Navigator.Viewport.CenterY}");
        }

        private async Task ShowVirtualTables(int linesCount)
        {
            int coef = linesCount > 2 ? 2 : linesCount;
            LazyVirtualTablesView.HeightRequest = Height * (coef + 1) * 0.20;


            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height - slideMenu.HeightRequest + 30);

            await LazyVirtualTablesView.TranslateTo(0, 0, 400);
            //await map.LayoutTo(new Rect(0, 0, Width, Height - slideMenu.HeightRequest + 30));
            map.HeightRequest = Height - LazyVirtualTablesView.HeightRequest + 30;
            //AbsoluteLayout.SetLayoutFlags(map, AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlagks.WidthProportional);
            //AbsoluteLayout.SetLayoutBounds(map, new Rect(1, 1, 1, Height - slideMenu.HeightRequest + 30));
            //animation.Commit(map, "ShowMap", 256, 400);
        }

        private async Task HideVirtualTables()
        {
            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height);

            map.HeightRequest = Height;

            //AbsoluteLayout.SetLayoutFlags(map, AbsoluteLayoutFlags.All);
            //AbsoluteLayout.SetLayoutBounds(map, new Rect(1, 1, 1, 1));
            //await map.LayoutTo(new Rect(0, 0, Width, Height));
            await LazyVirtualTablesView.TranslateTo(0, Height, 400);
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


            MainThread.BeginInvokeOnMainThread(async () =>
            {
                isOpened = message.Show;

                if (!message.Show)
                    await HideVirtualTables();
                else if (message.Show)
                    await ShowVirtualTables(message.ArrivalsCount);
            });
        }

        private async Task LoadMapAsync()
        {
            mapService = ServiceContainer.ServiceProvider.GetService<MapService>();
            await (BindingContext as MapViewModel).Initialize(map.Map);

            map.TouchAction += OnMapTouchAction;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            LazyVirtualTablesView.TranslationY = isOpened ? 0 : Height;

            if (map != null)
                map.HeightRequest = Height;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (map != null)
            {
                map.IsVisible = false;
                map.IsVisible = true;
            }
        }
    }
}
