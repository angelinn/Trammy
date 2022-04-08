using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    public class MapViewModel : BaseViewModel
    {
        public ObservableCollection<ArrivalStopModel> RecommendedStops { get; private set; } = new ObservableCollection<ArrivalStopModel>();
        public ICommand MyLocationCommand { get; private set; }
        public ICommand OpenHamburgerCommand { get; private set; }
        public ICommand ShowMapCommand { get; private set; }

        private MapService mapService;

        public MapViewModel()
        {
            MyLocationCommand = new RelayCommand(async () => await OnMyLocationTappedAsync());
            OpenHamburgerCommand = new RelayCommand(() => MessengerInstance.Send(new SlideHamburgerMessage()));
            ShowMapCommand = new RelayCommand(() => MessengerInstance.Send(new ShowMapMessage(false)));

            mapService = SimpleIoc.Default.GetInstance<MapService>();

            MessengerInstance.Register<StopSelectedMessage>(this, OnStopSelectedMessageReceived);
            MessengerInstance.Register<FavouritesChangedMessage>(this, OnFavouritesChanged);
        }

        private bool isMapVisible;
        public bool IsMapVisible
        {
            get
            {
                return isMapVisible;
            }
            set
            {
                isMapVisible = value;
                RaisePropertyChanged();
            }
        }

        private bool isMyLocationVisible;
        public bool IsMyLocationVisible
        {
            get
            {
                return isMyLocationVisible;
            }
            set
            {
                isMyLocationVisible = value;
                RaisePropertyChanged();
            }
        }

        private string myLocationColor = "White";
        public string MyLocationColor
        {
            get
            {
                return myLocationColor;
            }
            set
            {
                myLocationColor = value;
                RaisePropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            await LocalizeAsync(true);
            IsMapVisible = true;
            IsMyLocationVisible = true;
        }

        private async Task OnMyLocationTappedAsync()
        {
            Task<bool> t = LocalizeAsync();

            MyLocationColor = "LightGray";
            await Task.Delay(100);
            MyLocationColor = "White";
            await Task.Delay(100);
        } 

        private async Task<bool> LocalizeAsync(bool first = false)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (ApplicationService.HasLocationPermissions())
                    {
                        Position position = await ApplicationService.GetCurrentPositionAsync();

                        ApplicationService.RunOnUIThread(() =>
                        {
                            mapService.MoveToUser(position, true);
                        });

                        MessengerInstance.Send(new UpdateLocationMessage(position));

                        return true;
                    }

                    return false;

                }
                catch (Exception ex)
                {
                    return false;
                }
            });
        } 

        private void OnFavouritesChanged(FavouritesChangedMessage message)
        {
            RecommendedStops.Clear();

            foreach (ArrivalStopModel favourite in message.Favourites.Take(5).Select(f => new ArrivalStopModel(f.StopCode, f.Name)))
            {
                RecommendedStops.Add(favourite);
            }
        }

        private void OnStopSelectedMessageReceived(StopSelectedMessage message)
        {
            if (message.Clicked)
                mapService.MoveToStop(message.Selected);
        }
    }
}
