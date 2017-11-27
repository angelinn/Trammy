using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages
{
    public class FavouriteAddedMessage
    {
        public FavouriteDomain Added { get; set; }

        public FavouriteAddedMessage(FavouriteDomain added)
        {
            Added = added;
        }
    }
}
