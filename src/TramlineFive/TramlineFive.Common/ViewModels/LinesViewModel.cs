using SkgtService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels
{
    public class LinesViewModel : BaseViewModel
    {
        public ObservableCollection<LineViewModel> Bus { get; set; } = new();
        public ObservableCollection<LineViewModel> Tram { get; set; } = new();
        public ObservableCollection<LineViewModel> Trolley { get; set; } = new();


        public LinesViewModel()
        {
            StopsLoader.LoadRoutesAsync().Wait();
            foreach (var item in StopsLoader.Routes)
            {
                List<LineViewModel> lines = new List<LineViewModel>();
                foreach (var route in item.Value)
                {
                    lines.Add(new LineViewModel { Type = item.Key, Routes = route.Value, Name = route.Key });
                }

                if (item.Key == "bus")
                    Bus = new(lines);
                else if (item.Key == "tram")
                    Tram = new(lines);
                else if (item.Key == "trolley")
                    Trolley = new(lines);
            }

        }
    }
}
