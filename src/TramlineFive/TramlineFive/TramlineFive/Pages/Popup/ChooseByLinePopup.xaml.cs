//using Rg.Plugins.Popup.Pages;
//using Rg.Plugins.Popup.Services;
//using SkgtService.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TramlineFive.ViewModels;
//using Xamarin.Forms;
//using Xamarin.Forms.Xaml;

//namespace TramlineFive.Pages.Popup
//{
//	[XamlCompilation(XamlCompilationOptions.Compile)]
//	public partial class ChooseByLinePopup : PopupPage
//	{
//        public ChooseByLineViewModel ChooseByLineViewModel { get; private set; }
//        public ChooseByLinePopup (IEnumerable<SkgtObject> lines)
//		{
//			InitializeComponent ();
//            ChooseByLineViewModel = new ChooseByLineViewModel(lines);

//            BindingContext = ChooseByLineViewModel;
//		}


//        private async void OnSelectedLineChanged(object sender, EventArgs e)
//        {
//            if (ChooseByLineViewModel.SelectedLine.SkgtValue != String.Empty)
//            {
//                await ChooseByLineViewModel.GetDirectionsAsync();
//            }
//        }

//        private async void OnSelectedDirectionChanged(object sender, EventArgs e)
//        {
//            if (ChooseByLineViewModel.SelectedDirection.SkgtValue != String.Empty)
//            {
//                await ChooseByLineViewModel.GetStopsAsync();
//            }
//        }

//        private async void OnSelectedStopChanged(object sender, EventArgs e)
//        {
//            if (ChooseByLineViewModel.SelectedStop.SkgtValue != String.Empty)
//            {
//                await ChooseByLineViewModel.ChooseStopAsync();
//            }
//        }

//        private async void OnCaptchaClicked(object sender, EventArgs e)
//        {
//            await GetTimingsAsync();
//        }

//        private async void OnCaptchaCompleted(object sender, EventArgs e)
//        {
//            await GetTimingsAsync();
//        }

//        private async Task GetTimingsAsync()
//        {
//            IEnumerable<string> timings = await ChooseByLineViewModel.GetTimingsAsync();
//            await PopupNavigation.PopAsync();
//        }

//}
//}