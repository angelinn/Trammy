﻿using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using Microsoft.Maui.Layouts;
using SkgtService.Models.Json;
using System;
using System.Threading.Tasks;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
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
            mapService.LoadInitialMap(map.Map).Wait();

            //map.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            WeakReferenceMessenger.Default.Register<ShowMapMessage>(this, async (r, m) => await ToggleMap(m));
            //Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
        }

        private async Task LoadDataAsync()
        {
            _ = LoadMapAsync();

            await ServiceContainer.ServiceProvider.GetService<HistoryViewModel>().LoadHistoryAsync();
            await ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>().LoadFavouritesAsync();
        }

        protected override async void OnAppearing()
        {
            if (initialized)
                return;

            initialized = true;

            _ = LoadDataAsync();

            DateTime versionCheckTime = Preferences.Get("VersionCheckDate", DateTime.MinValue);
            if (DateTime.Now - versionCheckTime < TimeSpan.FromDays(1))
                return;

            NewVersion version = await ServiceContainer.ServiceProvider.GetService<VersionService>().CheckForUpdates();
            if (version != null)
            {
                bool result = await DisplayAlert("Нова версия", $"{AppInfo.Name} има нова версия {version.VersionNumber} 🎉", "СВАЛЯНЕ", "ОТКАЗ");
                if (result)
                {
                    Uri url = new Uri(version.ReleaseUrl);
                    await Browser.Default.OpenAsync(url);
                }
            }

            Preferences.Set("VersionCheckDate", DateTime.Now);

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
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;


            //Animation animation = new Animation((h) => map.HeightRequest = h, map.HeightRequest, Height - slideMenu.HeightRequest + 30);

            await slideMenu.TranslateTo(0, 0, 400);
            //await map.LayoutTo(new Rect(0, 0, Width, Height - slideMenu.HeightRequest + 30));
            map.HeightRequest = Height - slideMenu.HeightRequest + 30;
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


            Dispatcher.Dispatch(async () =>
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

            await (BindingContext as MapViewModel).LoadAsync();

            if (VersionTracking.IsFirstLaunchEver)
                WeakReferenceMessenger.Default.Send(new ChangePageMessage("//Location"));
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            slideMenu.TranslationY = isOpened ? 0 : Height;

            if (map != null)
                map.HeightRequest = Height;
        }
    }
}
