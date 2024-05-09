using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;


using Mapsui;
using Microsoft.Extensions.DependencyInjection;
using SkgtService;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Models;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public partial class FavouritesViewModel : BaseViewModel
    {
        public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

        private readonly LocationService locationService;
        private bool firstLocalization = true;

        private readonly PublicTransport publicTransport;

        public FavouritesViewModel(LocationService locationService, PublicTransport publicTransport)
        {
            Messenger.Register<FavouriteAddedMessage>(this, (r, f) => OnFavouriteAdded(f.Value));

            Messenger.Register<StopSelectedMessage>(this, async (r, sc) => await OnStopSelected(sc.Value.Selected));
            Messenger.Register<UpdateLocationMessage>(this, async (r, message) =>
            {
                if (firstLocalization && ApplicationService.GetBoolSetting(Settings.ShowStopOnLaunch, true))
                {
                    firstLocalization = false;
                    await OnNearestFavouriteRequested(message.Position);
                }
            });

            this.locationService = locationService;
            this.publicTransport = publicTransport;
        }

        private async Task OnNearestFavouriteRequested(Position location)
        {
            double minDistance = double.MaxValue;
            FavouriteDomain minDistanceFavourite = null;

            while (Favourites == null)
                await Task.Delay(100);

            foreach (FavouriteDomain favourite in Favourites)
            { 
                StopInformation stop = MapService.Stops.FirstOrDefault(s => s.Code == favourite.StopCode);
                double distance = locationService.GetDistance(location.Latitude, location.Longitude, stop.Lat, stop.Lon);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceFavourite = favourite;
                }
            }

            if (minDistanceFavourite != null)
                Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(minDistanceFavourite.StopCode, true)));
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
            favourite.Lines = publicTransport.FindStop(favourite.StopCode).Lines;
            Favourites.Add(favourite);

            OnPropertyChanged(nameof(HasFavourites));

            ApplicationService.VibrateShort();
            Messenger.Send(new FavouritesChangedMessage(Favourites.ToList()));
        }

        [RelayCommand]
        private async Task RemoveFavouriteAsync(FavouriteDomain favourite)
        {
            if (await ApplicationService.DisplayAlertAsync("", $"Премахване на {favourite.Name}?", "Да", "Не"))
            {
                await FavouriteDomain.RemoveAsync(favourite.StopCode);
                Favourites.Remove(favourite);

                ApplicationService.DisplayToast($"{favourite.Name} е премахната");
                ApplicationService.VibrateShort();

                OnPropertyChanged(nameof(HasFavourites));

                Messenger.Send(new FavouritesChangedMessage(Favourites.ToList()));
            }
        }

        public async Task LoadFavouritesAsync()
        {
            IsLoading = true;

            Favourites = new ObservableCollection<FavouriteDomain>((await FavouriteDomain.TakeAsync()).OrderByDescending(f => f.TimesClicked));
            foreach (FavouriteDomain favourite in Favourites)
                favourite.Lines = publicTransport.FindStop(favourite.StopCode).Lines;

            IsLoading = false;

            OnPropertyChanged(nameof(Favourites));
            OnPropertyChanged(nameof(HasFavourites));

            Messenger.Send(new FavouritesChangedMessage(Favourites.ToList()));
        }

        public bool HasFavourites => (Favourites == null || Favourites.Count == 0) && !IsLoading;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private FavouriteDomain selected;

        partial void OnSelectedChanged(FavouriteDomain value)
        {
            if (value != null)
            {

                Messenger.Send(new ChangePageMessage("//Map"));
                Messenger.Send(new StopSelectedMessage(new StopSelectedMessagePayload(Selected.StopCode, true)));

                Selected = null;
                OnPropertyChanged();
            }
        }

    }
}
