
using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TramlineFive.Common.Services;
using TramlineFive.Common.Models;
using SkgtService;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}

namespace TramlineFive.Common.ViewModels.Locator
{
    public class ViewModelLocator
    {

        public VirtualTablesViewModel VirtualTablesViewModel => ServiceContainer.ServiceProvider.GetService<VirtualTablesViewModel>(); // SimpleIoc.Default.GetInstance<VirtualTablesViewModel>();
        public HistoryViewModel HistoryViewModel => ServiceContainer.ServiceProvider.GetService<HistoryViewModel>(); // SimpleIoc.Default.GetInstance<HistoryViewModel>();
        public HamburgerViewModel HamburgerViewModel => ServiceContainer.ServiceProvider.GetService<HamburgerViewModel>(); // SimpleIoc.Default.GetInstance<HamburgerViewModel>();
        public AboutViewModel AboutViewModel => ServiceContainer.ServiceProvider.GetService<AboutViewModel>(); // SimpleIoc.Default.GetInstance<AboutViewModel>();
        public SettingsViewModel SettingsViewModel => ServiceContainer.ServiceProvider.GetService<SettingsViewModel>(); // => ServiceContainer.ServiceProvider.GetService<SettingsViewModel>(); // SimpleIoc.Default.GetInstance<SettingsViewModel>();
        public FavouritesViewModel FavouritesViewModel => ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>(); // => ServiceContainer.ServiceProvider.GetService<FavouritesViewModel>(); // SimpleIoc.Default.GetInstance<FavouritesViewModel>();
        public LicensesViewModel LicensesViewModel => ServiceContainer.ServiceProvider.GetService<LicensesViewModel>(); // => ServiceContainer.ServiceProvider.GetService<LicensesViewModel>(); // SimpleIoc.Default.GetInstance<LicensesViewModel>();
        public MapViewModel MapViewModel => ServiceContainer.ServiceProvider.GetService<MapViewModel>();
        public MainViewModel MainViewModel => ServiceContainer.ServiceProvider.GetService<MainViewModel>();// => ServiceContainer.ServiceProvider.GetService<MainViewModel>(); // SimpleIoc.Default.GetInstance<MainViewModel>();
        public LineViewModel LinesViewModel => ServiceContainer.ServiceProvider.GetService<LineViewModel>();
        public LineDetailsViewModel LineDetailsViewModel => ServiceContainer.ServiceProvider.GetService<LineDetailsViewModel>(); 
        public BusLinesViewModel BusLinesViewModel => ServiceContainer.ServiceProvider.GetService<BusLinesViewModel>();
        public TramLinesViewModel TramLinesViewModel => ServiceContainer.ServiceProvider.GetService<TramLinesViewModel>();
        public TrolleyLinesViewModel TrolleyLinesViewModel => ServiceContainer.ServiceProvider.GetService<TrolleyLinesViewModel>();
        public ForwardLineDetailsViewModel ForwardLineDetailsViewModel => ServiceContainer.ServiceProvider.GetService<ForwardLineDetailsViewModel>();
        public DirectionsViewModel DirectionsViewModel => ServiceContainer.ServiceProvider.GetService<DirectionsViewModel>();


    }

    public static class ViewModelExtensions
    {
        public static void AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MapViewModel>();
            services.AddSingleton<VirtualTablesViewModel>();
            services.AddSingleton<HistoryViewModel>();
            services.AddSingleton<HamburgerViewModel>();
            services.AddSingleton<AboutViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<FavouritesViewModel>();
            services.AddSingleton<LicensesViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<BusLinesViewModel>();
            services.AddSingleton<TramLinesViewModel>();
            services.AddSingleton<TrolleyLinesViewModel>();
            services.AddSingleton<LineDetailsViewModel>();
            services.AddSingleton<ForwardLineDetailsViewModel>();
            services.AddSingleton<DirectionsViewModel>();
        }
    }
}
