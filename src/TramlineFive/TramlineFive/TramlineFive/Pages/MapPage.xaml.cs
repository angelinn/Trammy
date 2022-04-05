using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mapsui;
using Mapsui.UI.Forms;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using System;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : Grid
    {
        private bool initialized;
        private bool isOpened;

        private readonly MapService mapService;

        public MapPage()
        {
            InitializeComponent();

            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap(m));
            Messenger.Default.Register<MapClickedMessage>(this, (m) => OnMapClicked());
            Messenger.Default.Register<RefreshMapMessage>(this, m => map.Refresh());
            Messenger.Default.Register<UpdateLocationMessage>(this, m => map.MyLocationLayer.UpdateMyLocation(new Position(m.Position.Latitude, m.Position.Longitude)));

            Map nativeMap = new Map
            {
                BackColor = Mapsui.Styles.Color.White,
                CRS = "EPSG:3857"
            };

            mapService = SimpleIoc.Default.GetInstance<MapService>();
            mapService.Initialize(nativeMap, map.Navigator);

            map.MapClicked += OnMapClicked;
            map.Info += OnMapInfo;
            map.PinClicked += OnPinClicked;

            map.Map = nativeMap;
        }

        private void OnPinClicked(object sender, Mapsui.UI.Forms.PinClickedEventArgs e)
        {

        }

        private async void OnMapInfo(object sender, Mapsui.UI.MapInfoEventArgs e)
        {
            Messenger.Default.Send(new MapClickedResponseMessage(isOpened));

            if (isOpened)
                await HideVirtualTables();
            else
                mapService.OnMapInfo(sender, e);
        }

        private void OnMapClicked(object sender, Mapsui.UI.Forms.MapClickedEventArgs e)
        {

        }

        private void OnMapClicked()
        {

        }

        private async Task ShowVirtualTables(int linesCount)
        {
            if (isOpened)
                isOpened = false;

            int coef = linesCount > 2 ? 2 : linesCount;
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;

            await slideMenu.TranslateTo(0, 0, 400);

            isOpened = !isOpened;
        }

        private async Task HideVirtualTables()
        {
            await slideMenu.TranslateTo(0, Height, 400);

            isOpened = false;
        }

        private async Task ToggleMap(ShowMapMessage message)
        {
            if (!message.Show && isOpened)
                await HideVirtualTables();
            else if (message.Show)
                await ShowVirtualTables(message.ArrivalsCount);
        }

        public async Task OnAppearing()
        {
            if (initialized)
                return;

            initialized = true;
            await (BindingContext as MapViewModel).LoadAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            slideMenu.TranslationY = isOpened ? 0 : Height;
            map.HeightRequest = Height;
        }

        private void OnSearchFocused(object sender, FocusEventArgs e)
        {
            Messenger.Default.Send(new SearchFocusedMessage(true));
        }

        private void OnSearchUnfocused(object sender, FocusEventArgs e)
        {
            Messenger.Default.Send(new SearchFocusedMessage(false));
        }
    }
}
