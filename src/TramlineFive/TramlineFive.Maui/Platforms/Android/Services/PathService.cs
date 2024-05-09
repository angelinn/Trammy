
namespace TramlineFive.Maui.Services;

public partial class PathService
{
    public partial string GetDBPath() => Path.Combine(FileSystem.AppDataDirectory, "tramlinefive.db");
    public partial string GetBaseFilePath() => FileSystem.AppDataDirectory;
}
