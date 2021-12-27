using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using TramlineFive.UWP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(DatabasePathService))]
namespace TramlineFive.UWP.Services
{
    public class DatabasePathService : PathService
    {
        public string Path => System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "tramlinefive.db");

        public string BasePath => Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    }
}
