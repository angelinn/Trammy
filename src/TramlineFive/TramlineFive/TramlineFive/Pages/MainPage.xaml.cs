using GalaSoft.MvvmLight.Ioc;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace TramlineFive.Pages
{
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        private bool appeared;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                appeared = true;
                
                await SimpleIoc.Default.GetInstance<VirtualTablesViewModel>().CheckForUpdatesAsync();
                await SimpleIoc.Default.GetInstance<HistoryViewModel>().LoadHistoryAsync();
                await SimpleIoc.Default.GetInstance<FavouritesViewModel>().LoadFavouritesAsync();
            }
        }

        private void OnPageChanged(object sender, EventArgs e)
        {
            On<Android>().SetIsSwipePagingEnabled(CurrentPage.GetType() != typeof(MapPage));
        }
    }
}
