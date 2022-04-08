using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class FavouritesChangedMessage
    {
        public List<FavouriteDomain> Favourites { get; set; }

        public FavouritesChangedMessage(List<FavouriteDomain> favourites)
        {
            Favourites = favourites;
        }
    }
}
