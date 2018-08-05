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

            Messenger.Default.Register<ShowMapMessage>(this, async (m) => await ToggleMap(m.ArrivalsCount));
        }

        private async Task ToggleMap(int arrivalsCount = 0)
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
                int coef = arrivalsCount > 2 ? 2 : arrivalsCount;
                slideMenu.HeightRequest = Height * (coef + 1 ) * 0.20;

                vtSlide = slideMenu.TranslateTo(0, 0);
                mapSlide = Task.Run(() => map.Animate("height", resizeMap, map.Height, Height - (Height * coef * 0.23)));
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
            slideMenu.TranslationY = isOpened ? 0 : Height;
            map.HeightRequest = Height;
        }
    }
}
