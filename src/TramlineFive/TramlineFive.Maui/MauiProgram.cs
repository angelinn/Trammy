using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.LifecycleEvents;
using SkgtService.Parsers;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels.Locator;
using TramlineFive.Maui.Services;
using TramlineFive.Services.Main;
using SkgtService;

namespace TramlineFive.Maui
{
    public static class MauiProgram
    {
        private static IServiceProvider ServiceProvider;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSentry(options =>
                {
                    // The DSN is the only required setting.
                    options.Dsn = "https://692bacbf5e2846da6b7aeeb741412f4a@o4507219539394560.ingest.de.sentry.io/4507219541950544";

                    // Use debug mode if you want to see what the SDK is doing.
                    // Debug messages are written to stdout with Console.Writeline,
                    // and are viewable in your IDE's debug console or with 'adb logcat', etc.
                    // This option is not recommended when deploying your application.
                    options.Debug = true;

                    // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
                    // We recommend adjusting this value in production.
                    options.TracesSampleRate = 1.0;

                    // Sample rate for profiling, applied on top of othe TracesSampleRate,
                    // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                    // We recommend adjusting this value in production.
                    options.ProfilesSampleRate = 1.0;
                })
                .UseSkiaSharp(true)
                .ConfigureFonts(fonts =>
                {
                    //fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    //fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome6BrandsRegular400.otf", "FAB");
                    fonts.AddFont("FontAwesome6FreeRegular400.otf", "FAR");
                    fonts.AddFont("FontAwesome6FreeSolid900.otf", "FAS");
                    //fonts.AddFont("MaterialIconsOutlinedRegular.otf", "MaterialIconsOutlinedRegular.otf");
                    fonts.AddFont("MaterialIconsRegular.ttf", "mi");
                    fonts.AddFont("MaterialIconsRoundRegular.otf", "mir");
                    fonts.AddFont("MaterialIconsSharpRegular.otf", "mis");
                    fonts.AddFont("MaterialIconsTwoToneRegular.otf", "mit");
                })
                .UseMauiCommunityToolkit()
                .UseMauiCompatibility();

#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            });
#endif
            ConfigureServices(builder.Services);

#if DEBUG
		    builder.Logging.AddDebug();
#endif

            Mapsui.UI.Maui.MapControl.UseGPU = true;
            return builder.Build();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<WeatherService>();
            services.AddSingleton<SofiaHttpClient>();
            services.AddSingleton<PublicTransport>();
            services.AddSingleton<DirectionsService>();
            services.AddSingleton<ArrivalsService>();
            services.AddSingleton<LocationService>();
            services.AddSingleton<MapService>();
            services.AddSingleton<VersionService>();
            services.AddViewModels();

            services.AddSingleton<PathService>();
            services.AddSingleton<VersionCheckingService>();
            services.AddSingleton<PermissionService>();
            services.AddSingleton<PushService>();


            ServiceProvider = services.BuildServiceProvider();
            ServiceContainer.ServiceProvider = ServiceProvider;
        }
    }
}