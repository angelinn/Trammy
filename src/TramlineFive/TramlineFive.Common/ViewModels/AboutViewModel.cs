using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private string version;
        public string Version
        {
            get
            {
                if (String.IsNullOrEmpty(version))
                    version = ApplicationService.GetVersion();

                return version;
            }
            set
            {
                version = value;
            }
        }
    }
}
