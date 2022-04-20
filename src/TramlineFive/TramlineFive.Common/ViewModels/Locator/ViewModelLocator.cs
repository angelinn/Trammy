using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}

namespace TramlineFive.Common.ViewModels.Locator
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<VirtualTablesViewModel>();
            SimpleIoc.Default.Register<HistoryViewModel>();
            SimpleIoc.Default.Register<HamburgerViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<FavouritesViewModel>();
            SimpleIoc.Default.Register<LicensesViewModel>();
            SimpleIoc.Default.Register<MapViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public VirtualTablesViewModel VirtualTablesViewModel => SimpleIoc.Default.GetInstance<VirtualTablesViewModel>();
        public HistoryViewModel HistoryViewModel => SimpleIoc.Default.GetInstance<HistoryViewModel>();
        public HamburgerViewModel HamburgerViewModel => SimpleIoc.Default.GetInstance<HamburgerViewModel>();
        public AboutViewModel AboutViewModel => SimpleIoc.Default.GetInstance<AboutViewModel>();
        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstance<SettingsViewModel>();
        public FavouritesViewModel FavouritesViewModel => SimpleIoc.Default.GetInstance<FavouritesViewModel>();
        public LicensesViewModel LicensesViewModel => SimpleIoc.Default.GetInstance<LicensesViewModel>();
        public MapViewModel MapViewModel => SimpleIoc.Default.GetInstance<MapViewModel>();
        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();
    }
}
