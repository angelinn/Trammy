using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ChooseLinePopup : PopupPage
	{
        public ChooseLineViewModel LinesPickViewModel { get; private set; }
		public ChooseLinePopup(IEnumerable<Line> lines)
		{
			InitializeComponent ();

            LinesPickViewModel = new ChooseLineViewModel(lines);
            BindingContext = LinesPickViewModel;
		}
        
        private async void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (LinesPickViewModel.SelectedLine.SkgtValue != String.Empty)
            {
                await LinesPickViewModel.ChooseLineAsync();
            }
        }
        
        private async void OnCaptchaClicked(object sender, EventArgs e)
        {
            await GetTimingsAsync();
        }

        private async void OnCaptchaCompleted(object sender, EventArgs e)
        {
            await GetTimingsAsync();
        }

        private async Task GetTimingsAsync()
        {
            IEnumerable<string> timings = await LinesPickViewModel.GetTimingsAsync();
            await PopupNavigation.PopAsync();
        }
    }
}
