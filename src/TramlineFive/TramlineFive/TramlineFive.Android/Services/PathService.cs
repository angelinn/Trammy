using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TramlineFive.Services;
using TramlineFive.Droid.Services;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(TramlineFive.Droid.Services.PathService))]
namespace TramlineFive.Droid.Services
{
    public class PathService : IPathService
    {
        public string DBPath => 
            System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "tramlinefive.db");
        public string BaseFilePath => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

    }
}
