using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.ViewModels;
using Xamarin.Forms;

namespace TramlineFive.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel MainViewModel { get; private set; } = new MainViewModel();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = MainViewModel;
        }

        private async void OnCheckClicked(object sender, EventArgs e)
        {
            await MainViewModel.LoadLinesAsync();
        }
    }
}
