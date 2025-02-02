using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Microsoft.Maui.Layouts;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.Services.Maps;
using TramlineFive.Common.ViewModels;
using TramlineFive.Maui;
using Color = Microsoft.Maui.Graphics.Color;
using Map = Mapsui.Map;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace TramlineFive.Pages
{
    public partial class MapPage : ContentPage
    {
        private bool initialized;
        private bool hasMoved;

        private MapViewModel mapViewModel;

        public MapPage()
        {
            InitializeComponent();

            mapViewModel = ServiceContainer.ServiceProvider.GetService<MapViewModel>();
            mapViewModel.LoadInitialMap(map.Map,
                Preferences.Get(Settings.SelectedTileServerUrl, Defaults.TileServerUrl),
                Preferences.Get(Settings.FetchingStrategy, Defaults.DataFetchStrategy),
                Preferences.Get(Settings.RenderStrategy, Defaults.RenderFetchStrategy));

            map.UpdateInterval = 8;

            WeakReferenceMessenger.Default.Register<StopSelectedMessage>(this, async (r, m) => await ShowVirtualTables());
            WeakReferenceMessenger.Default.Register<StopDataLoadedMessage>(this, (r, m) => OnStopDataLoaded(m));
            WeakReferenceMessenger.Default.Register<ShowRouteMessage>(this, async (r, m) => await HideVirtualTables());
            WeakReferenceMessenger.Default.Register<HideVirtualTablesMessage>(this, async (r, m) => await HideVirtualTables());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (initialized)
                return;

            initialized = true;

            if (Application.Current.RequestedTheme == AppTheme.Dark)
                themeOverlay.Opacity = 0.3;

            if (VersionTracking.IsFirstLaunchEver)
                Navigation.PushAsync(new LocationPromptPage());

            _ = Task.Run(async () =>
            {
                await mapViewModel.SetupFullMapAsync();
                await Task.Delay(500);

                _ = mapViewModel.LocalizeAsync(true);

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

        private async Task ShowVirtualTables()
        {
            await LazyVirtualTablesView.TranslateTo(0, 0, 400);
        }

        private async Task HideVirtualTables()
        {
            await LazyVirtualTablesView.TranslateTo(0, Height, 400);

            Dispatcher.Dispatch(() =>
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Application.Current.RequestedTheme == AppTheme.Light ? Colors.DodgerBlue : Color.FromArgb("2d333b"))
            );
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            LazyVirtualTablesView.TranslationY = mapViewModel.IsVirtualTablesUp ? 0 : Height;
            LazyVirtualTablesView.HeightRequest = Height * 0.60;
            mapViewModel.OverlayHeightInPixels = LazyVirtualTablesView.HeightRequest;

#if ANDROID
            int statusBarHeight = 0;
            int resourceId = Android.Content.Res.Resources.System.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                statusBarHeight = Android.Content.Res.Resources.System.GetDimensionPixelSize(resourceId);
                double statusBarHeightDip = statusBarHeight / Android.Content.Res.Resources.System.DisplayMetrics.Density;

                mapViewModel.OverlayHeightInPixels -= statusBarHeightDip;
            }
#endif
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Application.Current.RequestedTheme == AppTheme.Light ? Colors.DodgerBlue : Color.FromArgb("2d333b"));
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(CommunityToolkit.Maui.Core.StatusBarStyle.LightContent);
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);

            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Application.Current.RequestedTheme == AppTheme.Light ? Colors.DodgerBlue : Color.FromArgb("2d333b"));
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(CommunityToolkit.Maui.Core.StatusBarStyle.LightContent);
        }

        private void OnStopDataLoaded(StopDataLoadedMessage m)
        {
            if (m.stopInfo.Arrivals.Count > 0)
            {
                TransportType mostFrequentType = m.stopInfo.Arrivals
                    .GroupBy(a => a.VehicleType)
                    .OrderByDescending(g => g.Count())
                    .First()!.Key;

                string color = TransportConvertеr.TypeToColor(mostFrequentType, Application.Current.RequestedTheme == AppTheme.Light);
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Color.FromArgb(color));
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return mapViewModel.NavigateBack();
        }

        private async void TapGestureRecognizer_Tapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        {
            if (mapViewModel.IsVirtualTablesUp)
            {
                await HideVirtualTables();
            }

            Debug.WriteLine("Tapped");
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            Debug.WriteLine($"Pan updated: {e.TotalY}");
        }
    }
}
