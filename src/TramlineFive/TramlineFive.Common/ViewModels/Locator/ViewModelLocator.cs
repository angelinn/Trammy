using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;

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
            SimpleIoc.Default.Register<MapViewModel>();
        }

        public VirtualTablesViewModel VirtualTablesViewModel => SimpleIoc.Default.GetInstance<VirtualTablesViewModel>();
        public HistoryViewModel HistoryViewModel => SimpleIoc.Default.GetInstance<HistoryViewModel>();
        public HamburgerViewModel HamburgerViewModel => SimpleIoc.Default.GetInstance<HamburgerViewModel>();
        public AboutViewModel AboutViewModel => SimpleIoc.Default.GetInstance<AboutViewModel>();
        public SettingsViewModel SettingsViewModel => SimpleIoc.Default.GetInstance<SettingsViewModel>();
        public FavouritesViewModel FavouritesViewModel => SimpleIoc.Default.GetInstance<FavouritesViewModel>();
        public MapViewModel MapViewModel => SimpleIoc.Default.GetInstance<MapViewModel>();
    }
}
