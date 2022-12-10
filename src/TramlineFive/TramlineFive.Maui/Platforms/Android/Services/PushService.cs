using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TramlineFive.Services;

namespace TramlineFive.Maui.Services
{
    public partial class PushService
    {
        private const string CHANNEL_ID = "trams";
        private static Context context;

        public static void SetContext(Context context)
        {
            PushService.context = context;
        }

        public partial void PushNotification(string title, string message)
        {
            Intent notificationIntent = new Intent(Intent.ActionView);
            //notificationIntent.SetData(Android.Net.Uri.Parse(urlData));
            PendingIntent pending = PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable);

            NotificationManager manager = NotificationManager.FromContext(context);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(CHANNEL_ID, "My Notifications", NotificationImportance.Max);

                // Configure the notification channel.
                notificationChannel.Description = "Channel description";
                notificationChannel.EnableLights(true);
                notificationChannel.LightColor = Android.Graphics.Color.AliceBlue;
                notificationChannel.SetVibrationPattern(new long[] { 0, 1000, 500, 1000 });
                notificationChannel.EnableVibration(true);
                manager.CreateNotificationChannel(notificationChannel);
            }

            Notification.Builder builder =
                new Notification.Builder(context, CHANNEL_ID)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(Android.Resource.Drawable.SymDefAppIcon);

            builder.SetContentIntent(pending);

            Notification notification = builder.Build();

            manager.Notify(1337, notification);
        }
    }
}
