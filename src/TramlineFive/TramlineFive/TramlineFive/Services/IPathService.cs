using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms.PlatformConfiguration;

namespace TramlineFive.Services
{
    public interface IPathService
    {
        string Path { get; }
        string BasePath { get; }
    }
}
