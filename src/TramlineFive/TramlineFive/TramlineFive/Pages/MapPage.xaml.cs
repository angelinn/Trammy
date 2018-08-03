using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
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

            Messenger.Default.Register<StopSelectedMessage>(this, async (m) => { if (!isOpened) await ToggleMap(); });
            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap());
        }

        private async Task ToggleMap()
        {
            void resizeMap(double i) => map.HeightRequest = i;

            Task vtSlide = null;
            Task mapSlide = null;

            if (isOpened)
            {
                vtSlide = slideMenu.TranslateTo(0, Height);
                mapSlide = Task.Run(() => map.Animate("height", resizeMap, map.Height, Height));
            }
            else
            {
                vtSlide = slideMenu.TranslateTo(0, 0);
                mapSlide = Task.Run(() => map.Animate("height", resizeMap, map.Height, 0.50 * Height));
            }
            
            await Task.WhenAll(vtSlide, mapSlide);
            overlay.InputTransparent = !overlay.InputTransparent;
            isOpened = !isOpened;
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
            slideMenu.TranslationY = Height;
            slideMenu.HeightRequest = 0.65 * Height;
            map.HeightRequest = Height;
        }
    }
}
