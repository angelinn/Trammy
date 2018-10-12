using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Ioc;
using SkgtService.Models;
using TramlineFive.Common.Services;
using TramlineFive.Services;

namespace TramlineFive.Droid.Services
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "com.company.BROADCAST" })]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (!SimpleIoc.Default.IsRegistered<IApplicationService>())
                {
                    Log.Info("VERSION", "Application service not registered. Registering new instance...");
                    SimpleIoc.Default.Register<IApplicationService, ApplicationService>();
                }

                NewVersion version = await VersionService.CheckForUpdates();
                if (version != null)
                {
                    string message = intent.GetStringExtra("message");
                    string title = intent.GetStringExtra("title");

                    Intent notificationIntent = new Intent(Intent.ActionView);
                    notificationIntent.SetData(Android.Net.Uri.Parse(version.ReleaseUrl));
                    PendingIntent pending = PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.CancelCurrent);

                    Notification.Builder builder =
                        new Notification.Builder(context)
                            .SetContentTitle(title)
                            .SetContentText(message)
                            .SetSmallIcon(Resource.Drawable.icon)
                            .SetDefaults(NotificationDefaults.All)
                            .SetPriority((int)Notification.PriorityHigh);

                    builder.SetContentIntent(pending);

                    Notification notification = builder.Build();
                    NotificationManager manager = NotificationManager.FromContext(context);
                    manager.Notify(1337, notification);
                }
            }
            catch (Exception ex)
            {
                Log.Error("VERSION", ex.Message);
            }
            //PendingIntent pendingz = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            //AlarmManager alarmManager = context.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>();
            //alarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 10 * 1000, pendingz);
        }
    }
}
