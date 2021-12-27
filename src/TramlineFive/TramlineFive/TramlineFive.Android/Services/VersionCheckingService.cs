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
using TramlineFive.Droid.Services;
using TramlineFive.Services;

[assembly: Xamarin.Forms.Dependency(typeof(VersionCheckingService))]
namespace TramlineFive.Droid.Services
{
    public class VersionCheckingService : IVersionCheckingService
    {
        private const long CHECK_TIME = AlarmManager.IntervalDay;
        public void CreateTask()
        {
            MainActivity mainActivity = Xamarin.Forms.Forms.Context as MainActivity;
            CreateTask(mainActivity);
        }

        public void CreateTask(MainActivity mainActivity)
        {
            if (!IsAlarmCreated())
            {
                Intent alarmIntent = new Intent(mainActivity, typeof(AlarmReceiver));
                alarmIntent.PutExtra("title", "Налична е нова версия");
                alarmIntent.PutExtra("message", "Натиснете тук за сваляне на новата версия.");

                PendingIntent pending = PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                AlarmManager alarmManager = mainActivity.GetSystemService(Context.AlarmService).JavaCast<AlarmManager>();
                alarmManager.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + CHECK_TIME, CHECK_TIME, pending);
            }
        }

        private bool IsAlarmCreated()
        {
            MainActivity mainActivity = Xamarin.Forms.Forms.Context as MainActivity;
            
            Intent alarmIntent = new Intent(mainActivity, typeof(AlarmReceiver));
            alarmIntent.PutExtra("title", "Налична е нова версия");
            alarmIntent.PutExtra("message", "Натиснете тук за сваляне.");
            PendingIntent pending = PendingIntent.GetBroadcast(mainActivity, 0, alarmIntent, PendingIntentFlags.NoCreate | PendingIntentFlags.Immutable);

            if (pending != null)
                Log.Info("VERSION", "Pending is not null (alarm is already created)");
            else
                Log.Info("VERSION", "Pending is null (alarm not created)");

            return (pending != null);
        }
    }
}
