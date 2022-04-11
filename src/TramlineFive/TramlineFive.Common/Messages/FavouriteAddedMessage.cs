using System;
using System.Collections.Generic;
using System.Text;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.Messages;

public record FavouriteAddedMessage(FavouriteDomain Added);
