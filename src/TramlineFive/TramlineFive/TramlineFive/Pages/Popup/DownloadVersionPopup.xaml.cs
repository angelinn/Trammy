using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DownloadVersionPopup : PopupPage
	{
		public DownloadVersionPopup (NewVersion newVersion)
		{
			InitializeComponent ();
            BindingContext = newVersion;
		}
        
        private void OnDownloadVersionTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri((BindingContext as NewVersion).ReleaseUrl));
        }

        public async void OnLaterClicked(object sender, EventArgs e)
        {
            await PopupNavigation.PopAsync();
        }
    }
}
