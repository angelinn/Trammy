using GalaSoft.MvvmLight.Messaging;
using TramlineFive.Common.Messages;
using TramlineFive.Common.ViewModels;
using TramlineFive.Common.Services;

namespace TramlineFive.Maui
{
    public partial class MainPage : ContentPage
    {
        private bool appeared;
        private bool isOpened;
        private IView Current => content.Children[0];
        public View View => content;
        public IList<IView> Children => content.Children;

        private View currentPage;
        public View CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
            }
        }

        public MainPage()
        {
            InitializeComponent();

            Messenger.Default.Register<SlideHamburgerMessage>(this, async (m) => await ToggleHamburgerAsync());
        }

        public async Task ToggleHamburgerAsync()
        {
            Task translation = null;
            Task fading = null;

            //if (!isOpened)
            //{
            //    translation = slideMenu.TranslateTo(0, 0);
            //    fading = overlay.FadeTo(0.5);
            //}
            //else
            //{
            //    translation = slideMenu.TranslateTo(-Width, 0);
            //    fading = overlay.FadeTo(0);
            //}

            await Task.WhenAll(translation, fading);
            //overlay.InputTransparent = !overlay.InputTransparent;
            isOpened = !isOpened;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            //slideMenu.TranslationX = -Width;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                mapPage.OnAppearing();
                Task task = ServiceContainer.ServiceProvider.GetService<VirtualTablesViewModel>().CheckForUpdatesAsync();

                appeared = true;
            }
        }

    }
}