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

        public async Task AddAsync(string name, string stopCode)
        {
            await TramlineFiveContext.AddAsync(new Favourite
            {
                Name = name,
                StopCode = stopCode
            });
        }

        public async Task<IEnumerable<FavouriteDomain>> Take(int count = 10)
        {
            return (await TramlineFiveContext.Take<Favourite>(count)).Select(f => new FavouriteDomain(f));
        }
    }
}
