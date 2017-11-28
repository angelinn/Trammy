using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Services;
using TramlineFive.UWP.Services;
using Windows.UI.Notifications;

[assembly: Xamarin.Forms.Dependency(typeof(ToastService))]
namespace TramlineFive.UWP.Services
{
    public class ToastService : IToastService
    {
        public void ShowToast(string message)
        {
            ToastContent content = new ToastContent
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = message
                            }
                        }
                    }
                }
            };

            ToastNotification toast = new ToastNotification(content.GetXml())
            {
                ExpirationTime = DateTime.Now.AddSeconds(5)
            };

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
