using GalaSoft.MvvmLight.Ioc;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Views
{
    public class MapsUIView : Xamarin.Forms.View
    {
        public Mapsui.Map NativeMap { get; }

        public MapsUIView()
        {
            NativeMap = new Mapsui.Map();
            NativeMap.BackColor = Color.White;

            SimpleIoc.Default.GetInstance<MapService>().Initialize(NativeMap);
        }
    }
}
