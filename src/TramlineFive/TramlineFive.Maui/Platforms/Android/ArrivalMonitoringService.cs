using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using SkgtService;
using Android.Net;
using Android.Media;

using Timer = System.Timers.Timer;
using Uri = Android.Net.Uri;

namespace TramlineFive.Maui.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
public class ArrivalMonitoringService : Service
{
    private Timer _timer;
    private GTFSClient gtfsClient;
    public static (string tripId, string routeId) Subscription { get; set; }
    private const int NOTIFY_BEFORE_MINUTES = 5;

    public override void OnCreate()
    {
        base.OnCreate();

        gtfsClient = IPlatformApplication.Current.Services.GetService<GTFSClient>();

        CreateNotificationChannel();
        CreateImportantNotificationChannel();

        var notif = new NotificationCompat.Builder(this, "poll_channel")
            .SetContentTitle("Transport Tracker")
            .SetContentText("Monitoring arrivals…")
            .SetSmallIcon(Resource.Drawable.bus_icon)
            .SetOngoing(true)
            .Build();

        StartForeground(1001, notif);

        _timer = new Timer(60_000);
        _timer.Elapsed += async (_, __) => await PollGtfsAsync();
        _timer.Start();
    }

    private async Task PollGtfsAsync()
    {
        try
        {
            await gtfsClient.QueryRealtimeData();

            bool any = false;
            var stopIds = gtfsClient.Stops.Where(s => s.StopCode == Subscription.routeId).Select(s => s.StopId).ToList();

            var key = gtfsClient.PredictedArrivals.Keys.FirstOrDefault(k => k.tripId == Subscription.tripId);
            foreach (string stopId in stopIds)
            {
                var sub = (Subscription.tripId, stopId);
                if (gtfsClient.PredictedArrivals.TryGetValue(sub, out DateTime predictedArrival))
                {
                    any = true;

                    var notifyTime = predictedArrival.AddMinutes(-NOTIFY_BEFORE_MINUTES);
                    if (DateTime.Now > notifyTime)
                    {
                        var notif = new NotificationCompat.Builder(this, "arrivals_channel")
    .SetContentTitle("Transport arriving soon!")
    .SetContentText($"Your bus arrives at {predictedArrival.ToLocalTime():HH:mm}.")
    .SetSmallIcon(Resource.Drawable.bus_icon)
    .SetPriority((int)NotificationPriority.High)
    .SetDefaults((int)(NotificationDefaults.Sound | NotificationDefaults.Vibrate))
    .SetAutoCancel(true)
    .Build();
                        var manager = (NotificationManager)GetSystemService(NotificationService);
                        manager.Notify(1002, notif);
                        StopForeground(true);
                        StopSelf();
                    }

                }
            }

            if (!any)
            {
                StopForeground(true);
                StopSelf();
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void CreateImportantNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelId = "arrivals_channel";
            var channelName = "Arrival Alerts";

            var channel = new NotificationChannel(
                channelId,
                channelName,
                NotificationImportance.High // 🔥 ensures heads-up popup + sound/vibration
            )
            {
                Description = "Notifications when your transport is arriving",
                LockscreenVisibility = NotificationVisibility.Public
            };

            channel.EnableVibration(true);
            channel.EnableLights(true);
            long[] pattern = { 0, 200, 150, 300, 150, 400 };
            channel.SetVibrationPattern(pattern);

            // Optionally assign a default notification sound
            var alarmSound = Uri.Parse(
                $"{ContentResolver.SchemeAndroidResource}://{ApplicationContext.PackageName}/{Resource.Raw.trolley}");
            var audioAttributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Notification)
                .SetContentType(AudioContentType.Sonification)
                .Build();

            channel.SetSound(alarmSound, audioAttributes);

            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }


    public override IBinder OnBind(Intent intent) => null;

    public override void OnDestroy()
    {
        _timer?.Stop();
        base.OnDestroy();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                "poll_channel", "GTFS Polling", NotificationImportance.Low);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }
}
