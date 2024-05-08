using CommunityToolkit.Mvvm.Messaging;
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
            Task _ = AnimateText();
        }

        private async Task AnimateText()
        {
            while (true)
            {
                if (txtStopName.Width > 0)
                {
                    await txtStopName.TranslateTo(-txtStopName.Width, 0, 5000);
                    txtStopName.TranslationX = Width;

                    await txtStopName.TranslateTo(0, 0, 5000);
                }

                await Task.Delay(5000);
            }
        }
    }
}
