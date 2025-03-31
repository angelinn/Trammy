using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.Common;

public static class Settings
{
    public const string ShowStopOnLaunch = "ShowStopOnLaunch";
    public const string MaxTextZoom = "MaxTextZoom";
    public const string MaxPinsZoom = "MaxPinsZoom";
    public const string StopsUpdated = "StopsUpdated";
    public const string SelectedTileServer = "SelectedTileServer";
    public const string SelectedTileServerUrl = "SelectedTileServerUrl";
    public const string Theme = "Theme";
    public const string FetchingStrategy = "FetchingStrategy";
    public const string RenderStrategy = "RenderStrategy";
    public const string MapCenterX = "CenterX";
    public const string MapCenterY = "CenterY";
}

public static class Defaults
{
    public const string TileServer = "carto-voyager";
    public const string TileServerUrl = "https://cartodb-basemaps-{s}.global.ssl.fastly.net/rastertiles/voyager/{z}/{x}/{y}@3x.png";
    public const string DataFetchStrategy = "DataFetchStrategy";
    public const string RenderFetchStrategy = "RenderFetchStrategy";

}
