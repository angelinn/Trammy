using GalaSoft.MvvmLight.Ioc;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;

namespace TramlineFive.Pages
{
    public partial class MainPage : TabbedPage
    {
        public VirtualTablesPage VirtualTablesPage { get; private set; } = new VirtualTablesPage();
        public HistoryPage HistoryPage { get; private set; } = new HistoryPage();

        private bool appeared;

        public MainPage()
        {
            InitializeComponent();
            
            Children.Add(VirtualTablesPage);
            Children.Add(HistoryPage);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                appeared = true;
                
                await SimpleIoc.Default.GetInstance<VirtualTablesViewModel>().CheckForUpdatesAsync();
                await SimpleIoc.Default.GetInstance<HistoryViewModel>().LoadHistoryAsync();
            }
        }
    }
}
