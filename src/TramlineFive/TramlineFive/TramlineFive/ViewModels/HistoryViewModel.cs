using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        public ObservableCollection<HistoryDomain> History { get; private set; }

        public async Task LoadHistoryAsync()
        {
            History = new ObservableCollection<HistoryDomain>(await HistoryDomain.TakeAsync());
            OnPropertyChanged("History");
        }
    }
}
