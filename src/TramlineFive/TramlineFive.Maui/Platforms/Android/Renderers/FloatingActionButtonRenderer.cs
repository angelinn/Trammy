using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using TramlineFive.Droid.Renderers;
using TramlineFive.Views;

using AndroidFloatingActionButton = Google.Android.Material.FloatingActionButton.FloatingActionButton; // Android.Support.Design.Widget.FloatingActionButton;

[assembly: ExportRenderer(typeof(FloatingActionButton), typeof(FloatingActionButtonRenderer))]
namespace TramlineFive.Droid.Renderers
{
    public class FloatingActionButtonRenderer : ViewRenderer<FloatingActionButton, AndroidFloatingActionButton>
    {
        public FloatingActionButtonRenderer(Context context) : base(context)
        {   }

        protected override void OnElementChanged(ElementChangedEventArgs<FloatingActionButton> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            AndroidFloatingActionButton floatingButton = new AndroidFloatingActionButton(Context);
            ViewCompat.SetBackgroundTintList(floatingButton, ColorStateList.ValueOf(Element.ButtonColor.ToAndroid()));
            floatingButton.UseCompatPadding = true;

            SetImage(floatingButton, Element.ImageSource);

            floatingButton.Click += OnFloatingButtonClick;
            SetNativeControl(floatingButton);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            Control.BringToFront();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AndroidFloatingActionButton floatingButton = Control;

            if (e.PropertyName == nameof(Element.ButtonColor))
                ViewCompat.SetBackgroundTintList(Control, ColorStateList.ValueOf(Element.ButtonColor.ToAndroid()));

            if (e.PropertyName == nameof(Element.ImageSource))
                SetImage(floatingButton, Element.ImageSource);

            base.OnElementPropertyChanged(sender, e);
        }

        private void OnFloatingButtonClick(object sender, EventArgs e)
        {
            (Element as IButtonController).SendClicked();
        }
        
        private void SetImage(AndroidFloatingActionButton floatingButton, ImageSource imageSource)
        {
            FileImageSource image = imageSource as FileImageSource;
            if (image != null)
            {
                floatingButton.SetImageDrawable(Context.GetDrawable(image.File));
            }
        }
    }
}
