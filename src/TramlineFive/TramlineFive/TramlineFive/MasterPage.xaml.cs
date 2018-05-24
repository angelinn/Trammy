using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.ViewModels;
using TramlineFive.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Xamarin.Forms.Grid;

namespace TramlineFive
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        private bool appeared;
        private View Current => content.Children[0];
        public View View => content;
        public IGridList<View> Children => content.Children;

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

        private bool isOpened;

        public MasterPage()
        {
            InitializeComponent();

            Messenger.Default.Register<SlideHamburgerMessage>(this, async (m) =>
            {
                await ToggleHamburgerAsync();
            }); 
        }

        private async void ContentTapped(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Content tapped");
            if (isOpened)
            {

                System.Diagnostics.Debug.WriteLine("toggling");
                await ToggleHamburgerAsync();
            }
        }

        private async Task ToggleHamburgerAsync()
        {
            Task translation = null;
            Task fading = null;
            if (!isOpened)
            {
                System.Diagnostics.Debug.WriteLine("slideMenu.TranslateTo(0, 0);");
                translation = slideMenu.TranslateTo(0, 0);
                fading = overlay.FadeTo(0.5);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("slideMenu.TranslateTo(-Width, 0);");
                translation = slideMenu.TranslateTo(-Width, 0);
                fading = overlay.FadeTo(0);
            }

            await Task.WhenAll(translation, fading);
            overlay.InputTransparent = !overlay.InputTransparent;
            isOpened = !isOpened;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            slideMenu.TranslationX = -Width;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                Task[] loadingTasks = new Task[]
                {
                    mapPage.OnAppearing(),
                    SimpleIoc.Default.GetInstance<VirtualTablesViewModel>().CheckForUpdatesAsync()
                };

                appeared = true;
                await Task.WhenAll(loadingTasks);
            }
        }

    }
}