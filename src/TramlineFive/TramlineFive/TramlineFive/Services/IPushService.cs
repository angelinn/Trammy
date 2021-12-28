using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Services
{
    public interface IPushService
    {
        void PushNotification(string title, string message);
    }
}
