using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;
using Xamarin.Forms;

namespace TramlineFive.Services
{
    public class ApplicationService : IApplicationService
    {
        public string GetVersion()
        {
            return Version.Plugin.CrossVersion.Current.Version.Substring(0, 5);
        }

        public void OpenUri(string uri)
        {
            Device.OpenUri(new Uri(uri));
        }
    }
}
