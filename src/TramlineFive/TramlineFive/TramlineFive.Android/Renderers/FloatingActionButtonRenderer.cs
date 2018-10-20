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
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using TramlineFive.Droid.Renderers;
using TramlineFive.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using AndroidFloatingActionButton = Android.Support.Design.Widget.FloatingActionButton;

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

            SetImage(floatingButton, Element.Image);

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

            if (e.PropertyName == nameof(Element.Image))
                SetImage(floatingButton, Element.Image);

            base.OnElementPropertyChanged(sender, e);
        }

        private void OnFloatingButtonClick(object sender, EventArgs e)
        {
            (Element as IButtonController).SendClicked();
        }
        
        private void SetImage(AndroidFloatingActionButton floatingButton, FileImageSource imageSource)
        {
            FileImageSource image = imageSource;
            if (image != null)
            {
                floatingButton.SetImageDrawable(Context.GetDrawable(image.File));
            }
        }
    }
}
