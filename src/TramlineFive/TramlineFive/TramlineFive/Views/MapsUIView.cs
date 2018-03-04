using GalaSoft.MvvmLight.Ioc;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.Common.ViewModels;

namespace TramlineFive.Views
{
    public class MapsUIView : Xamarin.Forms.View
    {
        public Mapsui.Map NativeMap { get; }

        protected internal MapsUIView()
        {
            NativeMap = new Mapsui.Map();
            NativeMap.BackColor = Color.White;

            MapViewModel mapViewModel = SimpleIoc.Default.GetInstance<MapViewModel>();
            mapViewModel.Initialize(NativeMap);
        }
    }
}
