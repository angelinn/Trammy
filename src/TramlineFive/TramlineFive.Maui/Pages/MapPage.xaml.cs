using CommunityToolkit.Mvvm.Messaging;
using Mapsui.UI.Maui;
using SkgtService.Models;
using System.Diagnostics;
using TramlineFive.Common;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using SkiaSharp.Views.Maui.Controls;
using TramlineFive.Common.Services.Maps;
using Plugin.Maui.BottomSheet;
using Plugin.Maui.BottomSheet.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration;

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
                Preferences.Get(Settings.RenderStrategy, Defaults.RenderFetchStrategy),
                Preferences.Get(Settings.MapCenterX, double.MinValue),
                Preferences.Get(Settings.MapCenterY, double.MinValue));

            map.UpdateInterval = 8;

            SKGLView glview = typeof(MapControl).GetField("_glView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(map) as SKGLView;
            glview.Touch += OnMapTouch;

            WeakReferenceMessenger.Default.Register<StopDataLoadedMessage>(this, (r, m) => OnStopDataLoaded(m));
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
                    //LazyVirtualTablesView.LoadViewAsync();
                    LazyFab.LoadViewAsync();
                    LazySuggestions.LoadViewAsync();
                    sw.Stop();
                    Debug.WriteLine($"Lazy load time: {sw.ElapsedMilliseconds}");
                });
            });
        }

        private void OnMapTouch(object sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
        {
            MapViewModel.TouchType touchType = e.ActionType switch
            {
                SkiaSharp.Views.Maui.SKTouchAction.Pressed => MapViewModel.TouchType.Pressed,
                SkiaSharp.Views.Maui.SKTouchAction.Released => MapViewModel.TouchType.Released,
                SkiaSharp.Views.Maui.SKTouchAction.Moved => MapViewModel.TouchType.Moved,
                _ => MapViewModel.TouchType.None
            };

            _ = mapViewModel.OnMapTouchAsync(touchType);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            mapViewModel.OverlayHeightInPixels = Height * 0.6;

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
    }
}
