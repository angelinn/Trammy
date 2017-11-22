using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;

namespace TramlineFive.Services
{
    public class ApplicationService : IApplicationService
    {
        public string GetVersion()
        {
            return Version.Plugin.CrossVersion.Current.Version.Substring(0, 5);
        }
    }
}
