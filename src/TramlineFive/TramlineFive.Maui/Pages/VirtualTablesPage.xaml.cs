using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.BottomSheet.Navigation;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.ViewModels;
using TramlineFive.Services;

namespace TramlineFive.Pages
{
	public partial class VirtualTablesPage : Grid
	{
        private bool isLoaded = false;

        public VirtualTablesPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            WeakReferenceMessenger.Default.Register<StopSelectedMessage>(this, (r, m) =>
            {
                txtStopName.CancelAnimations();
                txtStopName.TranslationX = 0;
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
    }
}
