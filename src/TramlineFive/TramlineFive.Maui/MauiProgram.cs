using Android.Graphics.Drawables;
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
using Xamarin.CommunityToolkit.Effects;
using SkiaSharp.Views.Maui.Controls.Hosting;
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
                .UseMauiCompatibility()
                //.ConfigureMauiHandlers(handlers =>
                //{
                //    // Register ALL handlers in the Xamarin Community Toolkit assembly
                //    handlers.AddCompatibilityRenderers(typeof(Xamarin.CommunityToolkit.UI.Views.AvatarView).Assembly);
                //})
                .ConfigureEffects(effects =>
                {
                    effects.AddCompatibilityEffects(typeof(Xamarin.CommunityToolkit.Effects.TouchEffect).Assembly);
                });

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(global::Android.Graphics.Color.Transparent);
                handler.PlatformView.SetBackgroundDrawable(gd);
            });

            ConfigureServices(builder.Services);

#if DEBUG
		    builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<INavigationService, NavigationService>();

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