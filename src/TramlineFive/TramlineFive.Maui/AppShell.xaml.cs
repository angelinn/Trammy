using CommunityToolkit.Mvvm.Messaging;
using TramlineFive.Common.Messages;
using TramlineFive.Maui.Pages;
using TramlineFive.Pages;
using Microsoft.Maui.ApplicationModel;

namespace TramlineFive.Maui
{
    public partial class AppShell : Shell
    {
        private bool opened;

        public AppShell()
        {
            InitializeComponent(); 

            WeakReferenceMessenger.Default.Register<SlideHamburgerMessage>(this, (r, m) =>
            {
                // Workaround for opening the flyout
                if (!opened)
                {
                    FlyoutBehavior = FlyoutBehavior.Locked;
                    FlyoutBehavior = FlyoutBehavior.Flyout;

                    opened = true;
                    FlyoutIsPresented = false;
                }

                FlyoutIsPresented = !FlyoutIsPresented;
            });

            WeakReferenceMessenger.Default.Register<ChangePageMessage>(this, (r, m) => MainThread.BeginInvokeOnMainThread(() => GoToAsync($"{m.Page}")));
            Routing.RegisterRoute("schedule", typeof(SchedulesPage));
            Routing.RegisterRoute("Licenses", typeof(LicensesPage));
        }
    }
}
