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

            try
            {
                //SimpleIoc.Default.GetInstance<MapService>().Initialize(NativeMap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        void HeightAnimation(double i) => HeightRequest = i;
        
        public Animation AnimateHeightAsync(double from, double to)
        {
            return new Animation(HeightAnimation, from, to);
        }
    }
}
