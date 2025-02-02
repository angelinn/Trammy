using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages;

public record FavouritesChangedMessage(List<FavouriteDomain> Favourites);
public record RequestAddFavouriteMessage(string Name, string StopCode);
public record RequestDeleteFavouriteMessage(string StopCode);