using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.BottomSheet.Navigation;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using TramlineFive.Services;

namespace TramlineFive.Pages
{
	public partial class VirtualTablesPage : Grid
	{
        private bool isLoaded = false;
        private bool hasScrolled = false;

        public VirtualTablesPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            WeakReferenceMessenger.Default.Register<StopSelectedMessage>(this, (r, m) =>
            {
                txtStopName.CancelAnimations();
                txtStopName.TranslationX = 0;

                hasScrolled = false;
            });
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            if (!isLoaded)
            {
                isLoaded = true;
                Task _ = AnimateText();

                //refreshView.HeightRequest = DeviceDisplay.MainDisplayInfo.Height - 80;
            }
        }

        private async Task AnimateText()
        {
            while (true)
            {
                if (txtStopName.Width > 0)
                {
                    SizeRequest size = txtStopName.Measure(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

                    await txtStopName.TranslateTo(-size.Request.Width, 0, 3000);
                    await Task.Delay(100);

                    if (txtStopName.TranslationX == 0)
                        continue;

                    txtStopName.TranslationX = DeviceDisplay.MainDisplayInfo.Width - starView.Width;

                    await txtStopName.TranslateTo(0, 0, 5000);
                }

                await Task.Delay(5000);
            }
        }

        private void listView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (!hasScrolled)
                return;

            var listView = sender as ListView;
            var items = listView.ItemsSource as IList<RouteArrivalInformation>;  // Cast to your data type

            if (items != null && e.Item == items[items.Count - 1])
            {
                // ListView has scrolled to the bottom

                ServiceContainer.ServiceProvider.GetService<MapViewModel>().CurrentVirtualTablesState = MapViewModel.SheetState.Large;
                Console.WriteLine("Reached the bottom of the ListView!");
            }
        }

        private void listView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (e.ScrollY > 0)
                hasScrolled = true;
        }
    }
}
