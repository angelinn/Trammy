﻿using TramlineFive.Common.ViewModels;

namespace TramlineFive.Common.Messages;

public class MapLoadedMessage
{

}

public class HideVirtualTablesMessage
{

}

public record HeightChangedMessage(MapViewModel.SheetState SheetState);
public record ViewportChangedMessage(double x, double y);

