using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Rg.Plugins.Popup.Services;
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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VirtualTablesPage : Grid
	{
        public static BindableProperty QueryProperty = BindableProperty.Create<VirtualTablesPage, string>(
            v => v.Query, null, BindingMode.TwoWay, propertyChanged: QueryPropertyChanged);

        private static void QueryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            VirtualTablesPage current = bindable as VirtualTablesPage;
            (current.BindingContext as VirtualTablesViewModel).StopCode = newValue as string;
        }

        public string Query
        {
            get { return GetValue(QueryProperty) as string; }
            set { SetValue(QueryProperty, value); }
        }

        public VirtualTablesPage ()
		{
			InitializeComponent ();
            Messenger.Default.Register<StopSelectedMessage>(this, (s) => Query = s.Selected);
		}
    }
}
