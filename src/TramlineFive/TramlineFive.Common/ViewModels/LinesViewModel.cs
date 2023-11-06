using GalaSoft.MvvmLight.Command;
using NetTopologySuite.Index.HPRtree;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.ViewModels
{
    public class LinesViewModel : BaseViewModel
    {
        public ObservableCollection<LineViewModel> Bus { get; set; } = new();
        public ObservableCollection<LineViewModel> Tram { get; set; } = new();
        public ObservableCollection<LineViewModel> Trolley { get; set; } = new();

        private Dictionary<string, List<LineViewModel>> lines = new Dictionary<string, List<LineViewModel>>();

        public ICommand ItemSelectedCommand { get; private set; }
        public ICommand FilterTrolleyCommand { get; private set; }
        public ICommand FilterTramsCommand { get; private set; }
        public ICommand FilterBusesCommand { get; private set; }

        public string SearchText { get; set; }

        public LineViewModel SelectedLine { get; set; }


        public LinesViewModel()
        {
            ItemSelectedCommand = new RelayCommand(OpenDetails);
            FilterTrolleyCommand = new RelayCommand(FilterTrolleys);
            FilterTramsCommand = new RelayCommand(FilterTrams);
            FilterBusesCommand = new RelayCommand(FilterBuses);

            StopsLoader.LoadRoutesAsync().Wait();
            lines = new(StopsLoader.Routes.Select(p => 
                        new KeyValuePair<string, List<LineViewModel>>(p.Key, p.Value.Select(i => 
                            new LineViewModel { Type = p.Key, Routes = i.Value, Name = i.Key }
                        ).ToList())));

            foreach (var item in lines)
            {
                if (item.Key == "bus")
                    Bus = new(item.Value);
                else if (item.Key == "tram")
                    Tram = new(item.Value);
                else if (item.Key == "trolley")
                    Trolley = new(item.Value);
            }

        }

        private void OpenDetails()
        {
            NavigationService.GoToDetails(SelectedLine);
        }

        private void FilterTrolleys()
        {
            Trolley = new ObservableCollection<LineViewModel>(lines["trolley"].Where(t => t.Name.Contains(SearchText)));

            RaisePropertyChanged(nameof(Trolley));
        }

        private void FilterTrams()
        {
            Tram = new ObservableCollection<LineViewModel>(lines["tram"].Where(t => t.Name.Contains(SearchText)));

            RaisePropertyChanged(nameof(Tram));
        }

        private void FilterBuses()
        {
            Bus = new ObservableCollection<LineViewModel>(lines["bus"].Where(t => t.Name.Contains(SearchText)));

            RaisePropertyChanged(nameof(Bus));
        }
    }
}
