using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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

        public MapPage()
        {
            InitializeComponent();

            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap(m));
            Messenger.Default.Register<MapClickedMessage>(this, (m) => OnMapClicked());
        }

        private async void OnMapClicked()
        {
            Messenger.Default.Send(new MapClickedResponseMessage(isOpened));

            if (isOpened)
                await HideVirtualTables();
        }

        private async Task ShowVirtualTables(int linesCount)
        {
            if (isOpened)
                isOpened = false;

            int coef = linesCount > 2 ? 2 : linesCount;
            slideMenu.HeightRequest = Height * (coef + 1) * 0.20;

            Task vtSlide = slideMenu.TranslateTo(0, 0);
            Task mapSlide = map.AnimateHeightAsync(map.Height, Height - (Height * coef * 0.23));


            await Task.WhenAll(vtSlide, mapSlide);
            isOpened = !isOpened;
        }

        private async Task HideVirtualTables()
        {
            Task vtSlide = slideMenu.TranslateTo(0, Height);
            Task mapSlide = map.AnimateHeightAsync(map.Height, Height);

            await Task.WhenAll(vtSlide, mapSlide);
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
    }
}
