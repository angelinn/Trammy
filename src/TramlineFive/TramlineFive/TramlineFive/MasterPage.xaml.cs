using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.ViewModels;
using TramlineFive.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Xamarin.Forms.Grid;

namespace TramlineFive
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        private bool appeared;
        private View Current => content.Children[0];
        public View View => content;
        public IGridList<View> Children => content.Children;

        private View currentPage;
        public View CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
            }
        }

        public MasterPage()
        {
            InitializeComponent();

            Messenger.Default.Register<FocusSearchMessage>(this, (m) =>
            {
                search.Focus();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!appeared)
            {
                Task[] loadingTasks = new Task[]
                {
                    mapPage.OnAppearing(),
                    SimpleIoc.Default.GetInstance<VirtualTablesViewModel>().CheckForUpdatesAsync()
                };

                appeared = true;
                await Task.WhenAll(loadingTasks);
            }
        }
    }
}