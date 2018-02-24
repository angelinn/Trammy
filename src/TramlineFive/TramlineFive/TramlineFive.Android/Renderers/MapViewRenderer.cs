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
using Mapsui.UI.Android;
using TramlineFive.Droid.Renderers;
using TramlineFive.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MapsUIView), typeof(MapViewRenderer))]
namespace TramlineFive.Droid.Renderers
{
    public class MapViewRenderer : ViewRenderer<MapsUIView, MapControl>
    {
        private MapControl mapNativeControl;
        private MapsUIView mapViewControl;

        public MapViewRenderer(Context context) : base(context)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<MapsUIView> e)
        {
            base.OnElementChanged(e);

            if (mapViewControl == null && e.NewElement != null)
                mapViewControl = e.NewElement;

            if (mapNativeControl == null && mapViewControl != null)
            {
                mapNativeControl = new MapControl(Context, null);
                mapNativeControl.Map = mapViewControl.NativeMap;

                SetNativeControl(mapNativeControl);
            }
        }
    }
}
