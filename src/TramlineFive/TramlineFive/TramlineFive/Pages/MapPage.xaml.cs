using System;
using System.Threading.Tasks;
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

            Task.Run(async () =>
            {
                while (true)
                {
                    await slideMenu.TranslateTo(0, 0);

                    await Task.Delay(2000);

                    await slideMenu.TranslateTo(0, Height);
                    await Task.Delay(2000);
                }
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
