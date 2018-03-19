using Rg.Plugins.Popup.Services;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.ViewModels;
using TramlineFive.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VirtualTablesPage : Grid
	{
        public VirtualTablesPage ()
		{
			InitializeComponent ();
		}
    }
}
