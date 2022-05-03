using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.ViewModels;
using TramlineFive.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	public partial class VirtualTablesPage : Grid
	{
        public VirtualTablesPage()
        {
            InitializeComponent();
            Task _ = AnimateText();
        }

        private async Task AnimateText()
        {
            while (true)
            {
                await txtStopName.TranslateTo(0, 0, 5000);
                txtStopName.TranslationX = Width;
                await txtStopName.TranslateTo(0, 0, 5000);

                await Task.Delay(5000);
            }
        }
    }
}
