
namespace TramlineFive.Maui.Services;

public partial class PathService
{
    public partial string GetDBPath() => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "tramlinefive.db");
    public partial string GetBaseFilePath() => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
}
