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

        public MapPage()
        {
            InitializeComponent();

            Messenger.Default.Register<StopSelectedMessage>(this, async (m) =>
            {
                await slideMenu.TranslateTo(0, 0);
                overlay.InputTransparent = false;
            });

            Messenger.Default.Register<ShowMapMessage>(this, async (m) =>
            {
                await slideMenu.TranslateTo(0, Height);
                overlay.InputTransparent = true;
            });
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
            slideMenu.TranslationY = -Height;
            slideMenu.HeightRequest = 0.65 * Height;
        }
    }
}
