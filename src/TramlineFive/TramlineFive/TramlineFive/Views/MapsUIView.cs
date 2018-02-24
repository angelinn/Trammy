using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Views
{
    public class MapsUIView : Xamarin.Forms.View
    {
        public Mapsui.Map NativeMap { get; }

        protected internal MapsUIView()
        {
            NativeMap = new Mapsui.Map();
            NativeMap.BackColor = Color.White;
        }
    }
}
