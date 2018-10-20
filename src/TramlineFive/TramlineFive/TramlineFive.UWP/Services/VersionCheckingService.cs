using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using TramlineFive.UWP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(VersionCheckingService))]
namespace TramlineFive.UWP.Services
{
    public class VersionCheckingService : IVersionCheckingService
    {
        public void CreateTask()
        {

        }
    }
}
