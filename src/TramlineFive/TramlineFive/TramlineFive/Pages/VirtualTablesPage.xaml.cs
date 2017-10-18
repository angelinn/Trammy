using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Pages.Popup;
using TramlineFive.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VirtualTablesPage : ContentPage
	{
        public MainViewModel MainViewModel { get; private set; } = new MainViewModel();

        public VirtualTablesPage ()
		{
			InitializeComponent ();
            BindingContext = MainViewModel;
		}
        
        private async void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            //await MainViewModel.ChooseLineAsync();
        }

        private async void OnCheckClicked(object sender, EventArgs e)
        {
            try
            {
                //IEnumerable<Line> lines = await MainViewModel.LoadLinesAsync();
                IEnumerable<Line> lines = new List<Line>
                {
                    new Line("избор на линия", ""),
                    new Line("reis 107", "107"),
                    new Line("tramvai 11", "11")
                };
                ChooseLinePopup linesPickPopup = new ChooseLinePopup(lines);
                await PopupNavigation.PushAsync(linesPickPopup);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        private async void OnCaptchaClicked(object sender, EventArgs e)
        {
            await MainViewModel.GetTimingsAsync();
        }
    }
}