using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess.Domain
{
    public class FavouriteDomain
    {
        public string Name { get; set; }
        public string StopCode { get; set; }

        public FavouriteDomain(Favourite entity)
        {
            Name = entity.Name;
            StopCode = entity.StopCode;
        }

        public static async Task<FavouriteDomain> AddAsync(string name, string stopCode)
        {
            Favourite added = new Favourite
            {
                Name = name,
                StopCode = stopCode
            };

            await TramlineFiveContext.AddAsync(added);
            return new FavouriteDomain(added);
        }

        public static async Task<IEnumerable<FavouriteDomain>> TakeAsync(int count = 10)
        {
            return (await TramlineFiveContext.Take<Favourite>(count)).Select(f => new FavouriteDomain(f));
        }
    }
}
