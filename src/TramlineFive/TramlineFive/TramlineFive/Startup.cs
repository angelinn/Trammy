using Microsoft.Extensions.DependencyInjection;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Common.ViewModels.Locator;
using TramlineFive.Services.Main;

namespace TramlineFive
{
    public class Startup
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static void ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<ArrivalsService>();
            services.AddSingleton<LocationService>();
            services.AddSingleton<MapService>();
            services.AddSingleton<VersionService>();
            services.AddViewModels();

            ServiceProvider = services.BuildServiceProvider();
            ServiceContainer.ServiceProvider = ServiceProvider;
        }
    }
}
