using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TramlineFive.Common.Messages;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class FavouritesViewModel : BaseViewModel
    {
        public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public FavouritesViewModel()
        {
            MessengerInstance.Register<FavouriteAddedMessage>(this, (f) => OnFavouriteAdded(f.Added));
            RemoveCommand = new RelayCommand<FavouriteDomain>(async (f) => await RemoveFavouriteAsync(f));

            MessengerInstance.Register<StopSelectedMessage>(this, async (sc) => await OnStopSelected(sc.Selected));
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
