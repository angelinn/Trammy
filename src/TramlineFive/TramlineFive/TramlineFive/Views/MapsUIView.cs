using GalaSoft.MvvmLight.Ioc;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Services;
using TramlineFive.Common.ViewModels;
using Xamarin.Forms;

namespace TramlineFive.Views
{
    public class MapsUIView : Xamarin.Forms.View
    {
        public Mapsui.Map NativeMap { get; }

        public MapsUIView()
        {
            NativeMap = new Mapsui.Map();
            NativeMap.BackColor = Mapsui.Styles.Color.White;

            SimpleIoc.Default.GetInstance<MapService>().Initialize(NativeMap);
        }

        void HeightAnimation(double i) => HeightRequest = i;
        
        public Task AnimateHeightAsync(double from, double to)
        {
            return Task.Run(() => this.Animate("Height", HeightAnimation, from, to, 16, 750));
        }
    }
}
