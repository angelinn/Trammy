
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;

using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Resource = Android.Resource;
using SkgtService.Parsers;
using SkgtService.Models;

namespace TramlineFive.Maui;

[BroadcastReceiver(Label = "TramlineFive Widget", Exported = true)]
[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[IntentFilter(new string[] { "com.TramlineFive.Maui.action.BTN_PRESS" })]
[MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
public class AppWidget : AppWidgetProvider
{
    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        foreach (int id in appWidgetIds)
        {
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

            var intent = new Intent(context, typeof(AppWidget));
            intent.SetAction("com.TramlineFive.Maui.action.BTN_PRESS");
            //intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            // Register click event for the Background
            var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            widgetView.SetOnClickPendingIntent(Resource.Id.btnRefresh, piBackground);

            appWidgetManager.UpdateAppWidget(id, widgetView);
        }

        //var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
        //appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
    }

    public override async void OnReceive(Context context, Intent intent)
    {
        base.OnReceive(context, intent);

        if (intent.Action == "com.TramlineFive.Maui.action.BTN_PRESS")
        {
            ArrivalsService arrivals = new ArrivalsService();
            var res = await arrivals.GetByStopCodeAsync("0095");

            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);
            SetTextViewText(widgetView, res.Lines[0].Name + " " + res.Lines[0].Minutes);

            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            AppWidgetManager.GetInstance(context).UpdateAppWidget(me, widgetView);
        }
    }

    private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
    {
        var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

        SetTextViewText(widgetView, "Hi");
        RegisterClicks(context, appWidgetIds, widgetView);

        return widgetView;
    }

    private void SetTextViewText(RemoteViews widgetView, string text)
    { 
        widgetView.SetTextViewText(Resource.Id.widgetSmall, text);
    }

    private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
    {
        var intent = new Intent(context, typeof(AppWidget));
        intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
        intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

        // Register click event for the Background
        var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
        widgetView.SetOnClickPendingIntent(Resource.Id.btnRefresh, piBackground);
    }

}
