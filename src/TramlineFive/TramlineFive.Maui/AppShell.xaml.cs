using GalaSoft.MvvmLight.Messaging;
using TramlineFive.Common.Messages;
using TramlineFive.Maui.Pages;

namespace TramlineFive.Maui
{
    public partial class AppShell : Shell
    {
        private bool opened;

        public AppShell()
        {
            InitializeComponent();

            Messenger.Default.Register<SlideHamburgerMessage>(this, (m) =>
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

            Messenger.Default.Register<ChangePageMessage>(this, (m) => GoToAsync($"//{m.Page}"));
            Routing.RegisterRoute("linedetails", typeof(LineDetails));
        }
    }
}