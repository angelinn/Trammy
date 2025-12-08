using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using TramlineFive.Maui.Platforms.Android;

namespace TramlineFive.Maui.Services;

public partial class MonitoringService
{
    public partial void Subscribe(string tripId, string stopId)
    {
        ArrivalMonitoringService.Subscription = (tripId,  stopId);

        var context = Platform.CurrentActivity;
        var intent = new Intent(context, typeof(ArrivalMonitoringService));
        context.StartForegroundService(intent);
    }

    public partial void StopSubscription()
    {
        var context = Platform.CurrentActivity;
        context.StopService(new Intent(context, typeof(ArrivalMonitoringService)));
    }
}
