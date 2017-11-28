using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TramlineFive.Droid.Services;
using TramlineFive.Services;

[assembly: Xamarin.Forms.Dependency(typeof(ToastService))]
namespace TramlineFive.Droid.Services
{
    public class ToastService : IToastService
    {
        private static Context context;

        public static void Init(Context mainContext)
        {
            context = mainContext;
        }

        public void ShowToast(string message)
        {
            Toast.MakeText(context, message, ToastLength.Short).Show();
        }
    }
}
