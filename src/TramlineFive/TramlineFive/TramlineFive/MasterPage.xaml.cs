using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;
using TramlineFive.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        private MapPage mapPage = new MapPage();
        private VirtualTablesPage vtPage = new VirtualTablesPage();
        private HistoryPage historyPage = new HistoryPage();
        private FavouritesPage favouritesPage = new FavouritesPage();

        private View Current => content.Children[0];
        public MasterPage()
        {
            InitializeComponent();

            vtPage.IsVisible = false;
            favouritesPage.IsVisible = false;
            historyPage.IsVisible = false;

            content.Children.Add(mapPage);
            content.Children.Add(vtPage);
            content.Children.Add(favouritesPage);
            content.Children.Add(historyPage);
        }
        bool appeared;
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await mapPage.OnAppearing();
            if (!appeared)
            {
                appeared = true;

                await SimpleIoc.Default.GetInstance<VirtualTablesViewModel>().CheckForUpdatesAsync();
                await SimpleIoc.Default.GetInstance<HistoryViewModel>().LoadHistoryAsync();
                await SimpleIoc.Default.GetInstance<FavouritesViewModel>().LoadFavouritesAsync();
            }
        }

        private void mapClicked(object sender, EventArgs e)
        {
                ChangeCurrentPage(mapPage);
        }
        
        private void ChangeCurrentPage(Grid page)
        {
            foreach (View child in content.Children)
            {
                if (child.Equals(page))
                    child.IsVisible = true;
                else
                    child.IsVisible = false;
            }
        }


        private void vtClicked(object sender, EventArgs e)
        {
                ChangeCurrentPage(vtPage);
        }
        private void favClicked(object sender, EventArgs e)
        {
            ChangeCurrentPage(favouritesPage);
        }
        private void hClicked(object sender, EventArgs e)
        {
            ChangeCurrentPage(historyPage);
        }
    }
}