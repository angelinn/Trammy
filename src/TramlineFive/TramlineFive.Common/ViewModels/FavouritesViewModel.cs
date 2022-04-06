using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using SkgtService.Models.Locations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain; 

namespace TramlineFive.Common.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        private readonly LocationService locationService;

        public FavouritesViewModel()
        {
            MessengerInstance.Register<FavouriteAddedMessage>(this, (f) => OnFavouriteAdded(f.Added));
            RemoveCommand = new RelayCommand<FavouriteDomain>(async (f) => await RemoveFavouriteAsync(f));

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) => await OnStopSelected(sc.Selected));
            MessengerInstance.Register<NearestFavouriteRequestedMessage>(this, async (pos) => await OnNearestFavouriteRequested(pos.Position));

            locationService = SimpleIoc.Default.GetInstance<LocationService>();
        }

        private async Task OnNearestFavouriteRequested(Position location)
        {
            double minDistance = double.MaxValue;
            FavouriteDomain minDistanceFavourite = null;

            while (Favourites == null)
                await Task.Delay(100);

            foreach (FavouriteDomain favourite in Favourites)
            { 
                StopLocation stop = MapService.Stops.FirstOrDefault(s => s.Code == favourite.StopCode);
                double distance = locationService.GetDistance(location.Latitude, location.Longitude, stop.Lat, stop.Lon);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceFavourite = favourite;
                }
            }

            if (minDistanceFavourite != null)
                MessengerInstance.Send(new StopSelectedMessage(minDistanceFavourite.StopCode, false));
        }

        private async Task OnStopSelected(string stopCode)
        {
            FavouriteDomain favourite = Favourites.FirstOrDefault(f => f.StopCode == stopCode);
            if (favourite != null)
            {
                await FavouriteDomain.IncrementAsync(favourite.StopCode);
                ++favourite.TimesClicked;
            }
        }

        private void OnFavouriteAdded(FavouriteDomain favourite)
        {
            Favourites.Add(favourite);
            RaisePropertyChanged("HasFavourites");
        }

        private async Task RemoveFavouriteAsync(FavouriteDomain favourite)
        {
            if (await ApplicationService.DisplayAlertAsync("", $"Премахване на {favourite.Name}?", "Да", "Не"))
            {
                await FavouriteDomain.RemoveAsync(favourite.StopCode);
                Favourites.Remove(favourite);

                ApplicationService.DisplayToast($"{favourite.Name} е премахната");
                RaisePropertyChanged("HasFavourites");
            }
        }

        public async Task LoadFavouritesAsync()
        {
            IsLoading = true;

            Favourites = new ObservableCollection<FavouriteDomain>((await FavouriteDomain.TakeAsync()).Reverse().OrderByDescending(f => f.TimesClicked));
            RaisePropertyChanged("Favourites");
            RaisePropertyChanged("HasFavourites");

            IsLoading = false;
        }

        public bool HasFavourites => (Favourites == null || Favourites.Count == 0) && !isLoading;

        private FavouriteDomain selected;
        public FavouriteDomain Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                RaisePropertyChanged();

                if (value != null)
                {
                    SimpleIoc.Default.GetInstance<MainViewModel>().ChangeViewCommand.Execute("Map");
                    MessengerInstance.Send(new StopSelectedMessage(selected.StopCode, true));

                    selected = null;
                    RaisePropertyChanged();
                }
            }
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                RaisePropertyChanged();
            }
        }
    }
}
