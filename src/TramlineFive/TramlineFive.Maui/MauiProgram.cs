using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using SkgtService.Parsers;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels.Locator;
using TramlineFive.Maui.Services;
using TramlineFive.Services.Main;
using SkgtService;
using Plugin.LocalNotification;
using SkgtService.Models;
using TramlineFive.Common.Services.Maps;
using TramlineFive.Common.Services.Interfaces;
using Microsoft.Maui.Storage;
using Plugin.Maui.BottomSheet.Hosting;

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
                .UseBottomSheet()
                .UseSentry(options =>
                {
                    // The DSN is the only required setting.
                    //options.Dsn = "https://692bacbf5e2846da6b7aeeb741412f4a@o4507219539394560.ingest.de.sentry.io/4507219541950544";
                    //options.Debug = false;
                    options.Dsn = string.Empty;
                    options.InitializeSdk = false;
                    //options.AutoSessionTracking = false;
                    //options.SendDefaultPii = false;
                    //options.TracesSampleRate = 0;
                    //options.ProfilesSampleRate = 0;
                })
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
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
                //.UseMauiCompatibility()
                .UseLocalNotification();

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

            MauiApp app = builder.Build();

            ServiceContainer.ServiceProvider = app.Services;
            return app;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<WeatherService>();
            services.AddSingleton<SofiaHttpClient>();
            services.AddSingleton(provider =>
            {
                return new StopsConfigurator(FileSystem.AppDataDirectory, Preferences.Get("APIVersion", ""));
            });

            services.AddSingleton<StopsLoader>();
            services.AddSingleton<PublicTransport>();
            services.AddSingleton<RoutesLoader>();
            services.AddSingleton<DirectionsService>();
            services.AddSingleton<ArrivalsService>();
            services.AddSingleton<LocationService>();
            services.AddSingleton<MapService>();
            services.AddSingleton<VersionService>();
            services.AddViewModels();

            services.AddSingleton<VersionCheckingService>();
            services.AddSingleton<PermissionService>();
        }
    }
}