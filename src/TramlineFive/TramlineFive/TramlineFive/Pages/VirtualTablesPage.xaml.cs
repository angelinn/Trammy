using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            await MainViewModel.ChooseLineAsync();
        }

        private async void OnCheckClicked(object sender, EventArgs e)
        {
            await MainViewModel.LoadLinesAsync();
        }
        private async void OnCaptchaClicked(object sender, EventArgs e)
        {
            await MainViewModel.GetTimingsAsync();
        }
    }
}