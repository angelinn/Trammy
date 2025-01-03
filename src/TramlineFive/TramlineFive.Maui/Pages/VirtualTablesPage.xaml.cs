﻿using CommunityToolkit.Mvvm.Messaging;
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
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // workaround for collection view not appearing inside refresh view
            refreshView.HeightRequest = Height - 80;
        }

        private async Task AnimateText()
        {
            while (true)
            {
                if (txtStopName.Width > 0)
                {
                    SizeRequest size = txtStopName.Measure(Width, Height);

                    System.Diagnostics.Debug.WriteLine("TRANSLATING OUT OF VIEW");
                    await txtStopName.TranslateTo(-size.Request.Width, 0, 3000);

                    System.Diagnostics.Debug.WriteLine("WAITING OUT OF VIEW");
                    await Task.Delay(1000);

                    if (txtStopName.TranslationX == 0)
                        continue;

                    txtStopName.TranslationX = Width;

                    System.Diagnostics.Debug.WriteLine("TRANSLATING IN VIEW");
                    await txtStopName.TranslateTo(0, 0, 5000);
                }

                System.Diagnostics.Debug.WriteLine("WAITING IN VIEW");
                await Task.Delay(5000);
            }
        }
    }
}
