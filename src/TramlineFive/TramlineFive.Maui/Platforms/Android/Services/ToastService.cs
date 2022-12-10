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

namespace TramlineFive.Maui.Services
{
    public partial class ToastService
    {
        private static Context context;

        public static void Init(Context mainContext)
        {
            context = mainContext;
        }

        public partial void ShowToast(string message)
        {
            Toast.MakeText(context, message, ToastLength.Short).Show();
        }
    }
}
