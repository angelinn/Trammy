using GalaSoft.MvvmLight;
using SkgtService;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess.Domain
{
    public class FavouriteDomain : ObservableObject
    {
        public string Name { get; set; }
        public string StopCode { get; set; }
        public List<Line> Lines { get; set; }

        private int timesClicked;
        public int TimesClicked
        {
            get
            {
                return timesClicked;
            }
            set
            {
                timesClicked = value;
                RaisePropertyChanged();
            }
        }

        public FavouriteDomain(Favourite entity)
        {
            Name = entity.Name;
            StopCode = entity.StopCode;
            TimesClicked = entity.TimesClicked;
        }

        public void LoadLines()
        {
            Lines = StopsLoader.GetLinesForStop(StopCode);
        }

        public static async Task<FavouriteDomain> AddAsync(string name, string stopCode)
        {
            if ((await TramlineFiveContext.FindFavouriteAsync(stopCode)) != null)
                return null;

            Favourite added = new Favourite
            {
                Name = name,
                StopCode = stopCode,
                TimesClicked = 1
            };

            await TramlineFiveContext.AddAsync(added);
            return new FavouriteDomain(added);
        }

        public static async Task<IEnumerable<FavouriteDomain>> TakeAsync()
        {
            return (await TramlineFiveContext.TakeAll<Favourite>()).Select(f => new FavouriteDomain(f));
        }

        public static async Task RemoveAsync(string stopCode)
        {
            await TramlineFiveContext.RemoveFavouriteAsync(stopCode);
        }

        public static async Task IncrementAsync(string stopCode)
        {
            await TramlineFiveContext.IncrementFavouriteAsync(stopCode);
        } 

        public static async Task<FavouriteDomain> FindAsync(string stopCode)
        {
            Favourite favourite = await TramlineFiveContext.FindFavouriteAsync(stopCode);
            if (favourite == null)
                return null;

            return new FavouriteDomain(favourite);
        }
    }
}
