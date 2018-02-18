using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Services
{
    public interface IPermissionService
    {
        bool HasLocationPermissions();
        void RequestLocationPermissions();
    }
}
