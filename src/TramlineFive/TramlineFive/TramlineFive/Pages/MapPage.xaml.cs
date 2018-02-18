using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TramlineFive.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
		public MapPage ()
		{
			InitializeComponent ();

            IPermissionService permissionService = DependencyService.Get<IPermissionService>();
            if (!permissionService.HasLocationPermissions())
                permissionService.RequestLocationPermissions();
            else
                MyMap.IsShowingUser = true;
        }
	}
}