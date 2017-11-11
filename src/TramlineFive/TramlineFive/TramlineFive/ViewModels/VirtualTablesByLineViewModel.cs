using SkgtService;
using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TramlineFive.ViewModels
{
    public class Transport
    {
        public string DisplayName { get; set; }
        public TransportType TransportType { get; set; }
    }

    public class VirtualTablesByLineViewModel : BaseViewModel
    {
        public List<Transport> TransportTypes { get; private set; } = new List<Transport>()
        {
            new Transport { DisplayName = "Трамвай", TransportType = TransportType.Tram },
            new Transport { DisplayName = "Тролей", TransportType = TransportType.Trolley },
            new Transport { DisplayName = "Автобус", TransportType = TransportType.Bus }
        };

        public VirtualTablesByLineViewModel()
        {
            SelectedType = TransportTypes[0];
        }

        private Transport selectedType;
        public Transport SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                selectedType = value;
                OnPropertyChanged();
            }
        }

        public async Task<IEnumerable<Line>> GetLinesAsync()
        {
            return await SkgtManager.LineParser.GetLinesAsync(selectedType.TransportType);
        }
    }
}
