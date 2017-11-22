using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        public ObservableCollection<HistoryDomain> History { get; private set; }

        public async Task LoadHistoryAsync()
        {
            History = new ObservableCollection<HistoryDomain>(await HistoryDomain.TakeAsync());
            RaisePropertyChanged("History");

            HistoryDomain.HistoryAdded += OnHistoryAdded;
        }

        private void OnHistoryAdded(object sender, EventArgs e)
        {
            History.Insert(0, sender as HistoryDomain);
        }
    }
}
