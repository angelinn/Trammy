using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.ViewModels;
using Xamarin.Forms;

namespace TramlineFive.Pages
{
    public partial class MainPage : TabbedPage
    {
        public VirtualTablesByLinePage VirtualTablesByLinePage { get; private set; } = new VirtualTablesByLinePage();
        public VirtualTablesPage VirtualTablesPage { get; private set; } = new VirtualTablesPage();
        public HistoryPage HistoryPage { get; private set; } = new HistoryPage();

        private bool appeared;

        public MainPage()
        {
            InitializeComponent();

            //Children.Add(VirtualTablesByLinePage);
            Children.Add(VirtualTablesPage);
            Children.Add(HistoryPage);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                appeared = true;

                // await VirtualTablesByLinePage.VirtualTablesByLineViewModel.CheckForUpdatesAsync();
                await VirtualTablesPage.VirtualTablesViewModel.CheckForUpdatesAsync();
                await HistoryPage.HistoryViewModel.LoadHistoryAsync();
            }
        }
    }
}
