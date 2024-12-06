using CommunityToolkit.Mvvm.Messaging;
using TramlineFive.Common.Messages;
using TramlineFive.Maui.Pages;
using TramlineFive.Pages;

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
                }

                FlyoutIsPresented = !FlyoutIsPresented;
            });

            WeakReferenceMessenger.Default.Register<ChangePageMessage>(this, (r, m) => GoToAsync($"{m.Page}"));
            Routing.RegisterRoute("schedule", typeof(SchedulesPage));
        }
    }
}