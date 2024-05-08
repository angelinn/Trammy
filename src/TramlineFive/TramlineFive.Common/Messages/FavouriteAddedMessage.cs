using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages;

public class FavouriteAddedMessage : ValueChangedMessage<FavouriteDomain>
{
    public FavouriteAddedMessage(FavouriteDomain value) : base(value)
    {
    }
}
