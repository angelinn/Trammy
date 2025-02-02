using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TramlineFive.Common.Messages;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels;

public partial class FavouritesViewModel : BaseViewModel
{
    public ObservableCollection<FavouriteDomain> Favourites { get; private set; }

    private readonly WeatherService weatherService;

    public FavouritesViewModel(WeatherService weatherService)
    {
        Messenger.Register<StopSelectedMessage>(this, async (r, sc) => await OnStopSelected(sc.Selected));
        Messenger.Register<RequestAddFavouriteMessage>(this, async (r, m) => await AddFavouriteAsync(m.Name, m.StopCode));
        Messenger.Register<RequestDeleteFavouriteMessage>(this, async (r, m) => await RemoveFavouriteAsync(m.StopCode));

        this.weatherService = weatherService;
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

    public async Task<FavouriteDomain> AddFavouriteAsync(string name, string code)
    {
        FavouriteDomain favourite = await FavouriteDomain.AddAsync(name, code);
        if (favourite is null)
            return null;

        Favourites.Add(favourite);

        OnPropertyChanged(nameof(HasFavourites));

        ApplicationService.VibrateShort();
        Messenger.Send(new FavouritesChangedMessage(Favourites.ToList()));

        return favourite;
    }

    public async Task RemoveFavouriteAsync(string code)
    {
        FavouriteDomain favourite = Favourites.FirstOrDefault(f => f.StopCode == code);
        if (favourite != null)
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
    }

    public async Task LoadFavouritesAsync()
    {
        IsLoading = true;

        Favourites = new ObservableCollection<FavouriteDomain>((await FavouriteDomain.TakeAsync()).OrderByDescending(f => f.TimesClicked));

        IsLoading = false;

        OnPropertyChanged(nameof(Favourites));
        OnPropertyChanged(nameof(HasFavourites));

        Messenger.Send(new FavouritesChangedMessage(Favourites.ToList()));

        Forecast = await weatherService.GetWeather("Sofia, Bulgaria");
    }

    public bool HasFavourites => (Favourites == null || Favourites.Count == 0) && !IsLoading;

    [ObservableProperty]
    private Forecast forecast;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private FavouriteDomain selected;

    partial void OnSelectedChanged(FavouriteDomain value)
    {
        if (value != null)
        {

            Messenger.Send(new ChangePageMessage("//Map"));
            Messenger.Send(new StopSelectedMessage(Selected.StopCode));

            Selected = null;
            OnPropertyChanged();
        }
    }

}
