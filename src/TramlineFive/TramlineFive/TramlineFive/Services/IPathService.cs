using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms.PlatformConfiguration;

namespace TramlineFive.Services
{
    public interface IPathService
    {
        string DBPath { get; }
        string BaseFilePath { get; }
    }
}
