using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Org.Apache.Http.Entity;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Services;
using static Android.OS.PowerManager;
using Uri = Android.Net.Uri;

namespace TramlineFive.Maui.Platforms.Android;

[Service]
public class WatchService : Service
{
    public override IBinder OnBind(Intent intent)
    {
        throw new NotImplementedException();
    }

    private string NOTIFICATION_CHANNEL_ID = "1000";
    private int NOTIFICATION_ID = 1;
    private string NOTIFICATION_CHANNEL_NAME = "notification";

    private Timer timer;
    private int lastMinutes = 100;
    private WakeLock wakeLock;

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        if (intent.Action == "STOP")
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            mNotificationManager.Cancel(NOTIFICATION_ID);

            wakeLock.Release();
            StopForeground(true);

            return StartCommandResult.NotSticky;
        }

        PowerManager powerManager = GetSystemService(Context.PowerService) as PowerManager;
        wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "ServiceWakeLock");

        wakeLock.Acquire();

        string line = intent.GetStringExtra("line");
        string stop = intent.GetStringExtra("stop");

        var notifcationManager = GetSystemService(Context.NotificationService) as NotificationManager;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            createNotificationChannel(notifcationManager);
        }

        StartForeground(NOTIFICATION_ID, BuildNotification($"Линия {line} на спирка {stop}", "Абониран"));

        timer = new Timer(async (m) =>
        {
            ArrivalsService arrivals = ServiceContainer.ServiceProvider.GetService<ArrivalsService>();
            var res = await arrivals.GetByStopCodeAsync(stop);
            NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

            int mins = res.Arrivals.First(a => a.LineName == line).Minutes;
            int ma = lastMinutes;
            var a = this;
            if (lastMinutes < 1 && mins > lastMinutes)
            {
                wakeLock.Release();
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                mNotificationManager.Cancel(NOTIFICATION_ID);

                StopForeground(true);
            }

            lastMinutes = mins;

            mNotificationManager.Notify(NOTIFICATION_ID, BuildNotification($"Линия {line} на спирка {stop}", "Следващ: " + mins));
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        return StartCommandResult.NotSticky;
    }


    private Notification BuildNotification(string title, string text)
    {
        var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);
        notification.SetAutoCancel(false);
        notification.SetOngoing(true);
        notification.SetSmallIcon(Resource.Drawable.icon);
        notification.SetContentTitle(title);
        notification.SetContentText(text);
        notification.SetVibrate(new long[] { 100 });

        Intent intent = new Intent(this, typeof(WatchService));
        intent.SetAction("STOP");
        PendingIntent pStopSelf = PendingIntent.GetService(this, 0, intent, PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable);
        notification.AddAction(new NotificationCompat.Action(Resource.Drawable.icon, "Спиране", pStopSelf));

        notification.SetSound(Settings.System.DefaultNotificationUri);

        return notification.Build();
    }

    public override void OnDestroy()
    {
        if (wakeLock.IsHeld)
            wakeLock.Release();
    }

    private void createNotificationChannel(NotificationManager notificationMnaManager)
    {
        var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, NOTIFICATION_CHANNEL_NAME,
        NotificationImportance.High);
        channel.EnableVibration(true);
        channel.SetVibrationPattern(new long[] { 100 });
        AudioAttributes audioAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.Notification)
                    .Build();

        channel.SetSound(Uri.Parse("android.resource://" + ApplicationContext.PackageName + "/raw/trolley"), audioAttributes);

        notificationMnaManager.CreateNotificationChannel(channel);
    }
}
