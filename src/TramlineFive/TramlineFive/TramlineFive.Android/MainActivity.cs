using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using HockeyApp.Android;
using TramlineFive.Droid.Services;
using Android;
using GalaSoft.MvvmLight.Ioc;
using TramlineFive.Common.Services;
using TramlineFive.Services;
using Plugin.Iconize;
using Android.Content;

namespace TramlineFive.Droid
{
    [Activity(Label = "Tramline Five BETA", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.Window.RequestFeature(WindowFeatures.ActionBar);
            // Name of the MainActivity theme you had there before.
            // Or you can use global::Android.Resource.Style.ThemeHoloLight
            base.SetTheme(Resource.Style.MainTheme);

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs); // Could also be Resource.Id.tabs

            ToastService.Init(this);
            PermissionService.Init(this);

#if GORILLA_PLAYER
            LoadApplication(UXDivers.Gorilla.Droid.Player.CreateApplication(
                 this,
                 new UXDivers.Gorilla.Config("Good Gorilla")
                  .RegisterAssemblyFromType<TramlineFive.Common.ViewModels.Locator.ViewModelLocator>()
                  .RegisterAssemblyFromType<TramlineFive.App>()
                  .RegisterAssemblyFromType<Plugin.Iconize.IconLabel>()
                  .RegisterAssemblyFromType<Plugin.Iconize.Fonts.FontAwesomeModule>()));

            if (!SimpleIoc.Default.ContainsCreated<IApplicationService>())
            {
                SimpleIoc.Default.Register<IApplicationService>(() => new ApplicationService());
                SimpleIoc.Default.Register<IInteractionService>(() => new InteractionService());
                SimpleIoc.Default.Register<INavigationService>(() => new NavigationService());
            }
#else
            LoadApplication(new App());
#endif

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrashManager.Register(this, "b4327f88594740d8a58720734a080901");
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

